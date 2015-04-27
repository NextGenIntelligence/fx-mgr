/*
 * Copyright (c) 2010-2015, Eremex Ltd.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions 
 * are met:
 * 1. Redistributions of source code must retain the above copyright 
 *    notice, this list of conditions and the following disclaimer.  
 * 2. Redistributions in binary form must reproduce the above copyright 
 *    notice, this list of conditions and the following disclaimer in the 
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the company nor the names of any contributors 
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE EREMEX LTD. AND CONTRIBUTORS "AS IS" AND 
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE EREMEX LTD. OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS 
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigFramework
{
    public class BeforeAnalyzingEventArgs : EventArgs
    {
        public int count { get; private set; }

        public BeforeAnalyzingEventArgs(int count)
        {
            this.count = count;
        }
    }

    public delegate void BeforeAnalyzingHandler(object sender, BeforeAnalyzingEventArgs e);

    //--------------------------------------------------------------------------------

    public class ItemAnalyzedEventArgs : EventArgs
    {
        public bool abort { get; set; }
        public string filePath { get; private set; }

        public ItemAnalyzedEventArgs(string path)
        {
            this.abort = false;
            this.filePath = path;
        }
    }

    public delegate void ItemAnalyzedHandler(object sender, ItemAnalyzedEventArgs e);

    //--------------------------------------------------------------------------------

    public class AfterAnalyzingEventArgs : EventArgs
    {
        public bool success { get; private set; }

        public AfterAnalyzingEventArgs(bool success)
        {
            this.success = success;
        }
    }

    public delegate void AfterAnalyzingHandler(object sender, AfterAnalyzingEventArgs e);

    //--------------------------------------------------------------------------------

    public class MetadataStorage
    {
		private class MetadataFormat
		{
			public static bool Validate(YamlStream stream)
			{
				foreach (var node in stream.Documents)
				{
					if (!(node.RootNode is YamlMappingNode))
					{
						return false;
					}
				}
				return true;
			}
		}

        public event BeforeAnalyzingHandler BeforeAnalyzing;
        public event ItemAnalyzedHandler ItemAnalyzed;
        public event AfterAnalyzingHandler AfterAnalyzing;

        private readonly string[] extensions;
        private readonly IEnumerable<string> folders;
        private IList<KeyValuePair<string, YamlStream>> metadata;

        public MetadataStorage(IEnumerable<string> folders, string[] extensions)
        {
			if (folders == null)
			{
				throw new ArgumentNullException();
			}

            this.folders = folders;
            this.extensions = extensions;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get all files with given extension which are contained in specified folders.
        /// </summary>
		/// <exception cref="System.UnauthorizedAccessException">Specified folder cannot be read.</exception>
		/// <exception cref="System.IOException">Specified folder cannot be read.</exception>
		/// <exception cref="System.ArgumentException">Invalid argument(s).</exception>
		/// 
        private static void GetFiles(string path, string[] extensions, List<string> filesFound)
        {
            if (Directory.Exists(path))
            {
                foreach (string e in extensions)
                {
                    filesFound.AddRange(Directory.GetFiles(path, e));
                }

                foreach (string d in Directory.GetDirectories(path))
                {
                    GetFiles(d, extensions, filesFound);
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Metadata extraction helper.
        /// </summary>
		/// <exception cref="System.ArgumentException">Invalid argument(s).</exception>
		/// <exception cref="System.ObjectDisposedException">Synchronization error during parallel metadata analysis.</exception>
		/// <exception cref="System.AggregateException"></exception>
		/// <exception cref="System.OutOfMemoryException">Some input file is too large.</exception>
		/// <exception cref="System.OverflowException">Some input file is too large.</exception>
		/// 
        private bool AnalyzeItems(IEnumerable<string> files)
        {
            var po = new ParallelOptions();
            var cts = new CancellationTokenSource();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            var syncRoot = new object();
            bool result = true;

            metadata = new List<KeyValuePair<string, YamlStream>>();

            try
            {
                Parallel.ForEach(files, po, item =>
                //foreach (var item in files)
                {
					try
					{
						YamlStream metastream = Prosoft.FXMGR.Metadata.MetadataProvider.GetMetadata(item);

						if(!MetadataFormat.Validate(metastream)) throw new FormatException();

						lock (syncRoot)
						{
							metadata.Add(new KeyValuePair<string, YamlStream>(item, metastream));
						}

						if (ItemAnalyzed != null)
						{
							var e = new ItemAnalyzedEventArgs(item);
							ItemAnalyzed(this, e);

							if (e.abort)
							{
								cts.Cancel();
								po.CancellationToken.ThrowIfCancellationRequested();
							}
						}
					}
					catch (FormatException)
					{
						Logger.Log("Wrong metadata format in file: " + item);
					}
					catch (UnauthorizedAccessException)
					{
						Logger.Log("Can't access file: " + item);
					}
					catch (IOException)
					{
						Logger.Log("Can't read file: " + item);
					}
					catch (YamlException e)
					{
						Logger.Log(String.Format("Metadata syntax error in file {0} at {1}.", item, e.Start.Column.ToString()));
					}
					catch (NotSupportedException)
					{
						Logger.Log("Can't process metadata in file: " + item);
					}
                }
                );
            }
            catch (OperationCanceledException)
            {
                result = false;
                metadata = null;
            }

            return result;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Extracts metadata from all files from source folders.
        /// </summary>
		/// <exception cref="System.ArgumentException">Invalid argument(s).</exception>
		/// <exception cref="System.ObjectDisposedException">Synchronization error during parallel metadata analysis.</exception>
		/// <exception cref="System.AggregateException"></exception>
		/// <exception cref="System.OutOfMemoryException">Some input file is too large.</exception>
		/// <exception cref="System.OverflowException">Some input file is too large.</exception>
		/// <exception cref="System.UnauthorizedAccessException">Some input files cannot be accessed.</exception>
		/// <exception cref="System.IOException">Some input files cannot be accessed.</exception>
		/// 
        public bool AnalyzeSources()
        {
            var srcs = new List<string>();

            foreach (string p in folders)
            {
                GetFiles(p, extensions, srcs);
            }

            if (BeforeAnalyzing != null)
            {
                BeforeAnalyzingEventArgs e = new BeforeAnalyzingEventArgs(srcs.Count);
                BeforeAnalyzing(this, e);
            }

            bool parseResult = AnalyzeItems(srcs);

            if (AfterAnalyzing != null)
            {
                AfterAnalyzingEventArgs e = new AfterAnalyzingEventArgs(parseResult);
                AfterAnalyzing(this, e);
            }

            return parseResult;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return metadata as enumerable collection.
        /// </summary>
        public IEnumerable<KeyValuePair<string, YamlStream>> Metadata
        {
            get
            {
                return metadata;
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// For custom or generated interfaces there is possibility to add custom user metadata.
        /// </summary>
        public void AddGeneratedItem(string file, string intrface, string impl)
        {
			YamlStream yaml = new YamlStream();
			string yamlString = String.Format("{{interface: [{0}, {1}]}}", intrface, impl);
			TextReader r = new StringReader(yamlString);
			yaml.Load(r);
            metadata.Add(new KeyValuePair<string, YamlStream>(file, yaml));
        }
    }
}
