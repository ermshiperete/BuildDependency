// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Extensions;

namespace BuildDependency.Jenkins
{
	public class JenkinsXmlDeserializer: XmlDeserializer
	{
		public static void Register(RestClient restClient)
		{
			restClient.AddHandler("application/xml", new JenkinsXmlDeserializer());
		}

		private static bool TryGetFromString(string inputString, out object result, Type type)
		{
			var converter = TypeDescriptor.GetConverter(type);

			if (converter.CanConvertFrom(typeof(string)))
			{
				result = (converter.ConvertFromInvariantString(inputString));
				return true;
			}

			result = null;
			return false;
		}

		private void PopulateListFromElements(Type t, IEnumerable<XElement> elements, IList list)
		{
			foreach (var element in elements)
			{
				var item = CreateAndMap(t, element);
				list.Add(item);
			}
		}

		private object HandleListDerivative(object x, XElement root, string propName, Type type)
		{
			Type t;

			if (type.IsGenericType)
			{
				t = type.GetGenericArguments()[0];
			}
			else
			{
				t = type.BaseType.GetGenericArguments()[0];
			}

			var list = (IList)Activator.CreateInstance(type);
			var elements = root.Descendants(t.Name.AsNamespaced(Namespace));
			var name = t.Name;

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(Namespace);
				elements = root.Descendants(lowerName);
			}

			if (!elements.Any())
			{
				var camelName = name.ToCamelCase(Culture).AsNamespaced(Namespace);
				elements = root.Descendants(camelName);
			}

			if (!elements.Any())
			{
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == name);
			}

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(Namespace);
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == lowerName);
			}

			PopulateListFromElements(t, elements, list);

			// get properties too, not just list items
			// only if this isn't a generic type
			if (!type.IsGenericType)
			{
				Map(list, root.Element(propName.AsNamespaced(Namespace)) ?? root); // when using RootElement, the heirarchy is different
			}

			return list;
		}

		protected override object Map(object x, XElement root)
		{
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props)
			{
				var type = prop.PropertyType;
				var typeIsPublic = type.IsPublic || type.IsNestedPublic;

				if (!typeIsPublic || !prop.CanWrite)
					continue;

				var attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);
				XName name;

				if (attributes.Length > 0)
				{
					var attribute = (DeserializeAsAttribute)attributes[0];
					name = attribute.Name.AsNamespaced(Namespace);
				}
				else
				{
					name = prop.Name.AsNamespaced(Namespace);
				}

				var value = GetValueFromXml(root, name, prop);

				if (value == null || name.ToString().ToLowerInvariant() == "job" || name.ToString().ToLowerInvariant() == "view")
				{
					// special case for inline list items
					if (type.IsGenericType)
					{
						var genericType = type.GetGenericArguments()[0];
						var first = GetElementByName(root, genericType.Name) ?? GetElementByName(root, name);
						var list = (IList)Activator.CreateInstance(type);

						if (first != null)
						{
							var elements = root.Elements(first.Name);
							PopulateListFromElements(genericType, elements, list);
						}

						prop.SetValue(x, list, null);
					}
					continue;
				}

				// check for nullable and extract underlying type
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					// if the value is empty, set the property to null...
					if (String.IsNullOrEmpty(value.ToString()))
					{
						prop.SetValue(x, null, null);
						continue;
					}

					type = type.GetGenericArguments()[0];
				}

				if (type == typeof(bool))
				{
					var toConvert = value.ToString().ToLower();
					prop.SetValue(x, XmlConvert.ToBoolean(toConvert), null);
				}
				else if (type.IsPrimitive)
				{
					prop.SetValue(x, value.ChangeType(type, Culture), null);
				}
				else if (type.IsEnum)
				{
					var converted = type.FindEnumValue(value.ToString(), Culture);
					prop.SetValue(x, converted, null);
				}
				else if (type == typeof(Uri))
				{
					var uri = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
					prop.SetValue(x, uri, null);
				}
				else if (type == typeof(string))
				{
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(DateTime))
				{
					if (DateFormat.HasValue())
					{
						value = DateTime.ParseExact(value.ToString(), DateFormat, Culture);
					}
					else
					{
						value = DateTime.Parse(value.ToString(), Culture);
					}

					prop.SetValue(x, value, null);
				}
#if !PocketPC
				else if (type == typeof(DateTimeOffset))
				{
					var toConvert = value.ToString();

					if (!string.IsNullOrEmpty(toConvert))
					{
						DateTimeOffset deserialisedValue;

						try
						{
							deserialisedValue = XmlConvert.ToDateTimeOffset(toConvert);
							prop.SetValue(x, deserialisedValue, null);
						}
						catch (Exception)
						{
							object result;

							if (TryGetFromString(toConvert, out result, type))
							{
								prop.SetValue(x, result, null);
							}
							else
							{
								//fallback to parse
								deserialisedValue = DateTimeOffset.Parse(toConvert);
								prop.SetValue(x, deserialisedValue, null);
							}
						}
					}
				}
#endif
				else if (type == typeof(Decimal))
				{
					value = Decimal.Parse(value.ToString(), Culture);
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(Guid))
				{
					var raw = value.ToString();
					value = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(value.ToString());
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(TimeSpan))
				{
					var timeSpan = XmlConvert.ToTimeSpan(value.ToString());
					prop.SetValue(x, timeSpan, null);
				}
				else if (type.IsGenericType)
				{
					var t = type.GetGenericArguments()[0];
					var list = (IList)Activator.CreateInstance(type);
					var container = GetElementByName(root, name);

					if (container.HasElements)
					{
						var first = container.Elements().FirstOrDefault();
						var elements = container.Elements(first.Name);

						PopulateListFromElements(t, elements, list);
					}

					prop.SetValue(x, list, null);
				}
				else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
				{
					// handles classes that derive from List<T>
					// e.g. a collection that also has attributes
					var list = HandleListDerivative(x, root, prop.Name, type);
					prop.SetValue(x, list, null);
				}
				else
				{
					//fallback to type converters if possible
					object result;

					if (TryGetFromString(value.ToString(), out result, type))
					{
						prop.SetValue(x, result, null);
					}
					else
					{
						// nested property classes
						if (root != null)
						{
							var element = GetElementByName(root, name);

							if (element != null)
							{
								var item = CreateAndMap(type, element);
								prop.SetValue(x, item, null);
							}
						}
					}
				}
			}

			return x;
		}

		protected override XElement GetElementByName(XElement root, XName name)
		{
			return base.GetElementByName(root, name);
		}
	}
}
