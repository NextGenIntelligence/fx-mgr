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

using Prosoft.FXMGR.ConfigFramework;
using Prosoft.FXMGR.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigModules
{
    using FxInterface = KeyValuePair<string, string>;

    //--------------------------------------------------------------------------------
    //
    // Now only two option types are supported.
    //
    public enum OptionType
    {
        Integer,
        Enumeration
    }

    //--------------------------------------------------------------------------------
    //
    // Every option should contain associated type, name of its #define and descrition, so,
    // these fields forms the base class.
    //
    public abstract class BaseOption
    {
        public OptionType type { get; protected set; }
        public string description { get; protected set; }
        public string name { get; protected set; }
        public object Tag { get; set; }

        public BaseOption(string description, string name, OptionType type)
        {
			if (description == null || name == null)
				throw new ArgumentNullException();

            this.description = description;
            this.name = name;
            this.type = type;
        }
    }

    //--------------------------------------------------------------------------------
    //
    // Saved option class is used in order to save options into the file.
    //
    [Serializable]
    public struct SavedOption
    {
        public OptionType type { get; set; }
        public string name { get; set; }
        public object value { get; set; }
    }

    //--------------------------------------------------------------------------------
    //
    // Option representing constant integer value.
    //
    public class IntegerOption : BaseOption
    {
        public long value { get; set; }
        public long maximum { get; private set; }
        public long minimum { get; private set; }
        public long step { get; private set; }

        public IntegerOption(string description, string name, long value, long maximum, long minimum, long step)
            : base(description, name, OptionType.Integer)
        {
			if (value < minimum || value > maximum)
				throw new ArgumentOutOfRangeException();

            this.value = value;
            this.maximum = maximum;
            this.minimum = minimum;
            this.step = step;
        }
    }

    //--------------------------------------------------------------------------------
    //
    // Option representing finite set of non-sequential constant values.
    //
    public class EnumOption : BaseOption
    {
        public int index { get; set; } 
        public string[] friendlyName { get; private set; }
        public string[] value { get; private set; }

        public EnumOption(string description, string name, int index, string[] friendlyName, string[] value)
            : base(description, name, OptionType.Enumeration)
        {
			if (value == null || friendlyName == null)
				throw new ArgumentNullException();

			if (index < 0 || index >= value.Count())
				throw new ArgumentOutOfRangeException();

            this.index = index;
            this.friendlyName = friendlyName;
            this.value = value;
        }
    }

    //--------------------------------------------------------------------------------
    //
    // Configuration options manager class.
    //
    public class CfgOptions
    {
        //
        // Maps interface name to associated options.
        //
        private readonly IDictionary<FxInterface, List<BaseOption>> interfaceToOptions = new Dictionary<FxInterface, List<BaseOption>>();

        //
        // Maps option name to associated option class.
        //
        private IDictionary<string, BaseOption> actualOptions = new Dictionary<string, BaseOption>();

        //--------------------------------------------------------------------------------

        private void AddOptionsToMap(FxInterface key, IEnumerable<BaseOption> options)
        {
            List<BaseOption> temp;

            if (!interfaceToOptions.TryGetValue(key, out temp))
            {
                interfaceToOptions[key] = new List<BaseOption>(options);
            }
            else
            {
                temp.AddRange(options);
            }
        }

        //--------------------------------------------------------------------------------
		/// <summary>
        /// Construct internal mapping by metadata.
		/// </summary>
		/// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
		/// 
        public CfgOptions(IEnumerable<KeyValuePair<string, YamlStream>> metadata)
        {
            foreach (KeyValuePair<string, YamlStream> item in metadata)
            {
                FxInterface associatedInterface = DependencyManager.GetAssociatedInterface(item);
                IEnumerable<BaseOption> associatedOptions = OptionsFormat.GetModuleOptions(item.Value);

                AddOptionsToMap(associatedInterface, associatedOptions);
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Actual options will be set as options associated with given interfaces.
        /// </summary>
        public void UpdateOptions(IEnumerable<FxInterface> included)
        {
            actualOptions.Clear();

            foreach (var module in included)
            {
                foreach (BaseOption option in interfaceToOptions[module])
                {
                    actualOptions[option.name] = option;
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve set of actual options for configuration specified at previous call of UpdateOptions.
        /// </summary>
        public IEnumerable<BaseOption> Options
        {
            get
            {
                return actualOptions.Values;
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve set of options prepared to saved into file.
        /// Configuration is specified at previous call of UpdateOptions.
        /// </summary>
        public IEnumerable<SavedOption> GetOptionsToSave()
        {
            ICollection<SavedOption> optionsToSave = new List<SavedOption>();

            foreach (var option in actualOptions.Values)
            {
                object value = null;

                switch (option.type)
                {
                    case OptionType.Integer: 
                    { 
                        var temp = (IntegerOption)option; 
                        value = temp.value; 
                    } 
                    break;
                    case OptionType.Enumeration: 
                    { 
                        var temp = (EnumOption)option; 
                        value = temp.index; 
                    } 
                    break;
                }

                optionsToSave.Add(new SavedOption() { type = option.type, name = option.name, value = value });
            }

            return optionsToSave;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Load saved options into current configuration.
        /// All saved options should be present as actual, so, configuration set in previous call of
        /// UpdateOptions should contain ALL options passed.
        /// </summary>
		/// <exception cref="System.InvalidCastException">Inconsistent saved options.</exception>
		/// 
        public void LoadSavedOptions(IEnumerable<SavedOption> savedOptions)
        {
            foreach (SavedOption savedOption in savedOptions)
            {
                BaseOption option = actualOptions[savedOption.name];

                switch (savedOption.type)
                {
                    case OptionType.Integer:
                    {
                        IntegerOption integerOption = (IntegerOption)option;
                        integerOption.value = (long)savedOption.value;
                    } 
                    break;
                    case OptionType.Enumeration:
                    {
                        EnumOption enumOption = (EnumOption)option;
                        enumOption.index = (int)savedOption.value;
                    } 
                    break;
                }
            }
        }

		private class OptionsFormat
		{
			private static uint ConvertStringToUInt(string str)
			{
				return Convert.ToUInt32(str, (str.Length > 2 && str.Substring(0, 2) == "0x") ? 16 : 10);
			}

			//--------------------------------------------------------------------------------
			/// <summary>
			/// Get integer option from associated YAML mapping node.
			/// </summary>
			/// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
			/// 
			private static IntegerOption GetIntegerOption(YamlMappingNode integerOptionNode, string description, string name, uint defaultVal)
			{
				try
				{
					YamlSequenceNode rangeNode = (YamlSequenceNode)MetadataProvider.QueryMetadata(integerOptionNode, "range");
					YamlScalarNode stepNode = (YamlScalarNode)MetadataProvider.QueryMetadata((YamlMappingNode)integerOptionNode, "step");

					string minString = ((YamlScalarNode)rangeNode.Children[0]).Value;
					string maxString = ((YamlScalarNode)rangeNode.Children[1]).Value;
					uint step = 1;

					if (stepNode != null)
					{
						step = ConvertStringToUInt(stepNode.Value);
					}

					uint max = ConvertStringToUInt(maxString);
					uint min = ConvertStringToUInt(minString);

					return new IntegerOption(description, name, defaultVal, max, min, step);
				}
				catch(InvalidCastException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
				catch(NullReferenceException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
				catch(IndexOutOfRangeException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
				catch (OverflowException)
				{
                    throw new FormatException("Wrong data for option: " + name);
				}
			}

			//--------------------------------------------------------------------------------
			/// <summary>
			/// Get enum option from associated YAML mapping node.
			/// </summary>
			/// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
			/// 
			private static EnumOption GetEnumOption(YamlMappingNode enumOptionNode, string description, string name, int defaultVal)
			{
				try
				{
					YamlSequenceNode enumValuesNode = (YamlSequenceNode)MetadataProvider.QueryMetadata(enumOptionNode, "values");

					int enumValuesCount = enumValuesNode.Children.Count;
					string[] friendlyName = new string[enumValuesCount];
					string[] value = new string[enumValuesCount];

					for (int i = 0; i < enumValuesCount; ++i)
					{
						YamlMappingNode enumItemNode = (YamlMappingNode)enumValuesNode.Children[i];
						YamlScalarNode friendlyNameNode = (YamlScalarNode)enumItemNode.Children.Keys.First();
						YamlScalarNode valueNode = (YamlScalarNode)enumItemNode.Children.Values.First();

						friendlyName[i] = friendlyNameNode.Value;
						value[i] = valueNode.Value;
					}

					return new EnumOption(description, name, defaultVal, friendlyName, value);
				}
				catch (InvalidCastException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
				catch (NullReferenceException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
				catch (IndexOutOfRangeException)
				{
                    throw new FormatException("Wrong metadata format for option: " + name);
				}
			}

			//--------------------------------------------------------------------------------
			/// <summary>
			/// Get enum option from associated YAML mapping node.
			/// </summary>
			/// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
			/// 
			public static IEnumerable<BaseOption> GetModuleOptions(YamlStream stream)
			{
				List<BaseOption> optionsList = new List<BaseOption>();

				try
				{
					foreach (YamlDocument doc in stream.Documents)
					{
						YamlNode optionsNode = MetadataProvider.QueryMetadata((YamlMappingNode)doc.RootNode, "options");

						if (optionsNode != null)
						{
							YamlSequenceNode optionsSequenceNode = (YamlSequenceNode)optionsNode;

							foreach (YamlNode optionNode in optionsSequenceNode.Children)
							{
								BaseOption option = null;

								YamlMappingNode mapping = (YamlMappingNode)optionNode;
								YamlScalarNode optionName = (YamlScalarNode)mapping.Children.Keys.First();
								YamlMappingNode optionDescriptorNode = (YamlMappingNode)mapping.Children.Values.First();

								YamlScalarNode typeNode = (YamlScalarNode)MetadataProvider.QueryMetadata(optionDescriptorNode, "type");
								YamlScalarNode descriptionNode = (YamlScalarNode)MetadataProvider.QueryMetadata(optionDescriptorNode, "description");
								YamlScalarNode defaultValNode = (YamlScalarNode)MetadataProvider.QueryMetadata(optionDescriptorNode, "default");

								switch (typeNode.Value)
								{
									case "int":
									{
										uint defaultValue = ConvertStringToUInt(defaultValNode.Value);
										option = GetIntegerOption(optionDescriptorNode, descriptionNode.Value, optionName.Value, (uint)defaultValue);
									}
									break;
									case "enum":
									{
										int defaultValue = int.Parse(defaultValNode.Value);
										option = GetEnumOption(optionDescriptorNode, descriptionNode.Value, optionName.Value, defaultValue);
									}
									break;
								}

								if (option != null)
								{
									optionsList.Add(option);
								}
							}
						}
					}

					return optionsList;
				}
				catch (InvalidCastException)
				{
                    throw new FormatException("Wrong metadata format.");
				}
				catch (NullReferenceException)
				{
                    throw new FormatException("Wrong metadata format.");
				}
				catch (IndexOutOfRangeException)
				{
                    throw new FormatException("Wrong metadata format.");
				}
			}
		}
    }
}
