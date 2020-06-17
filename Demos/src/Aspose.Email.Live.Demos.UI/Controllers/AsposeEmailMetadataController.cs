using Aspose.Email.Live.Demos.UI.LibraryHelpers;
using Aspose.Email.Live.Demos.UI.Models;
using Aspose.Email.Mapi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Aspose.Email.Live.Demos.UI.FileProcessing;

namespace Aspose.Email.Live.Demos.UI.Controllers
{
	public class AsposeEmailMetadataController : EmailBase
	{
		static string[] AllowedTypes =
		{
			"Boolean",
			"DateTime",
			"Double",
			"Number",
			"Int32",
			"String"
		};

		static string[] Types =
		{
			"Boolean",
			"DateTime",
			"Double",
			"Number",
			"String",
			"StringArray",
			"ObjectArray",
			"ByteArray",
			"Other"
		};

		[HttpPost]
		[AcceptVerbs("GET", "POST")]
		public HttpResponseMessage Properties(string folderName, string fileName)
		{
			
			try
			{
				//var requestData = await Request.Content.ReadAsJsonAsync<EmailFileData>();

				fileName = Config.Configuration.WorkingDirectory + folderName + "/" + fileName;

				var metadata = GetAsposeEmailMetadata(fileName);
				return Request.CreateResponse(HttpStatusCode.OK, metadata);
			}
			catch (Exception ex)
			{
				
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		protected Response ProcessTask(string fileName, string folderName, string appName, Aspose.Email.Live.Demos.UI.FileProcessing.CustomSingleOrZipFileProcessor.ProcessFileDelegate handler)
		{
			Aspose.Email.Live.Demos.UI.Models.License.SetAsposeEmailLicense();
			var processor = new CustomSingleOrZipFileProcessor()
			{
				
				CustomFileProcessMethod =  handler
			};

			

			return processor.Process(folderName, fileName);
		}

		[HttpPost]
		[AcceptVerbs("GET", "POST")]
		public async Task<Response> Download()
		{
			string fileName = "empty";

			try
			{

				var requestData = await Request.Content.ReadAsJsonAsync<EmailFileData>();
				
				fileName = Path.Combine(Config.Configuration.WorkingDirectory, requestData.Id , requestData.FileName);
				var message = MapiHelper.GetMapiMessageFromFile(fileName);

				SetMetadata(message, requestData);

				return  ProcessTask(requestData.FileName, requestData.Id, "MetadataApp", (string inputFilePath, string outputFolderPath) =>
				{
					message.Save(Path.Combine(outputFolderPath, requestData.FileName));
				});
			}
			catch (Exception ex)
			{
				
				return new Response
				{
					Status = "500 " + ex.Message,
					StatusCode = 500
				};
			}
		}

		[HttpPost]
		public async Task<Response> Clear()
		{
			string fileName = "empty";

			try
			{
				var requestData = await Request.Content.ReadAsJsonAsync<EmailFileData>();
				fileName = Config.Configuration.WorkingDirectory + requestData.Id + "/" + requestData.FileName;
				var message = MapiHelper.GetMapiMessageFromFile(fileName);

				ClearMetadata(message);

				return  ProcessTask(requestData.FileName, requestData.Id, "MetadataApp", (string inputFilePath, string outputFolderPath) =>
				{
					message.Save(Path.Combine(outputFolderPath, requestData.FileName));
				});
			}
			catch (Exception ex)
			{
				
				return new Response
				{
					Status = "500 " + ex.Message,
					StatusCode = 500
				};
			}
		}

		private void ClearMetadata(MapiMessage mail)
		{
			Type t = mail.GetType();

			foreach (var prop in t.GetProperties())
			{
				if (prop.CanWrite && IsAllowedType(prop.PropertyType.Name))
				{
					switch (prop.PropertyType.Name)
					{
						case "Boolean":
							prop.SetValue(mail, false);
							break;
						case "DateTime":
							prop.SetValue(mail, DateTime.MinValue);
							break;
						case "Double":
							prop.SetValue(mail, 0d);
							break;
						case "Number":
							prop.SetValue(mail, 0);
							break;
						case "Int32":
							prop.SetValue(mail, 0);
							break;
						case "String":
							prop.SetValue(mail, string.Empty);
							break;
						default:
							break;
					}
				}
			}

			foreach (var item in mail.GetCustomProperties())
				mail.RemoveProperty(item.Key);
		}

		private void SetMetadata(MapiMessage mail, EmailFileData request)
		{
			Type t = mail.GetType();

			foreach (var metadataObj in request.Properties.BuiltInMetadataList)
			{
				if (metadataObj.Name != "Item")
				{
					var prop = t.GetProperty(metadataObj.Name);
					if (metadataObj.Changed)
						SetBuiltInPropertyValue(prop, mail, metadataObj.Value);
				}
			}

			var customProps = mail.GetCustomProperties();

			foreach (var item in customProps)
				mail.RemoveProperty(item.Key);

			foreach (var metadataObj in request.Properties.CustomMetadataList)
			{
				var name = metadataObj.Name;

				switch (Types[metadataObj.Type])
				{
					case "Double":
						mail.AddCustomProperty(name, Convert.ToDouble(metadataObj.Value));
						break;
					case "Boolean":
						mail.AddCustomProperty(name, Convert.ToBoolean(metadataObj.Value));
						break;
					case "Number":
						mail.AddCustomProperty(name, Convert.ToInt64(metadataObj.Value));
						break;
					case "DateTime":
						mail.AddCustomProperty(name, Convert.ToDateTime(metadataObj.Value));
						break;
					case "String":
						mail.AddCustomProperty(name, metadataObj.Value);
						break;
					default:
						mail.AddCustomProperty(name, (MapiPropertyType)Enum.Parse(typeof(MapiPropertyType), Types[metadataObj.Type]), Encoding.Unicode.GetBytes(metadataObj.Value));
						break;
				}
			}
		}
		private void SetBuiltInPropertyValue(PropertyInfo prop, Object props, string value)
		{
			try
			{
				Object obj = null;
				if (prop.PropertyType.Name == "Boolean")
				{
					obj = Boolean.Parse(value);
				}
				else if (prop.PropertyType.Name == "DateTime")
				{
					obj = DateTime.Parse(value);
				}
				else if (prop.PropertyType.Name == "Double")
				{
					obj = Double.Parse(value);
				}
				else if (prop.PropertyType.Name == "Number" || prop.PropertyType.Name == "Int32")
				{
					obj = Int32.Parse(value);
				}
				else if (prop.PropertyType.Name == "String")
				{
					obj = value;
				}
				prop.SetValue(props, obj);
			}
			catch (Exception) { }
		}

		class EmailFileData
		{
			[JsonProperty("id")]
			public string Id { get; set; }
			public string FileName { get; set; }

			[JsonProperty("properties")]
			public AsposeMetadataProperties Properties { get; set; }
		}

		public class AsposeMetadataProperties
		{
			///<Summary>
			/// get or set BuiltInMetadataList
			///</Summary>
			[JsonProperty("BuiltIn")]
			public List<AsposeMetadataObject> BuiltInMetadataList { get; set; }
			///<Summary>
			/// get or set CustomMetadataList
			///</Summary>
			[JsonProperty("Custom")]
			public List<AsposeMetadataObject> CustomMetadataList { get; set; }
		}

		public class AsposeMetadataObject
		{
			///<Summary>
			/// get or set PropertyId
			///</Summary>
			public int PropertyId { get; set; }
			///<Summary>
			/// get or set Property
			///</Summary>
			public string Name { get; set; }
			///<Summary>
			/// get or set Value
			///</Summary>
			public string Value { get; set; }
			///<Summary>
			/// get or set ValueType
			///</Summary>
			public int Type { get; set; }

			///<Summary>
			/// get or set Changed
			///</Summary>
			public bool Changed { get; set; }
		}

		private AsposeMetadataProperties GetAsposeEmailMetadata(string fileNamePath)
		{
			var formatInfo = Email.Tools.FileFormatUtil.DetectFileFormat(fileNamePath);

			Aspose.Email.Mapi.MapiMessage mail;
			if (formatInfo.FileFormatType == Email.FileFormatType.Eml)
				mail = Aspose.Email.Mapi.MapiMessage.Load(fileNamePath, new Email.EmlLoadOptions());
			else
				if (formatInfo.FileFormatType == Email.FileFormatType.Msg)
				mail = Aspose.Email.Mapi.MapiMessage.Load(fileNamePath, new Email.MsgLoadOptions());
			else
				throw new Exception("Invalid file format");

			int i = 0;
			List<AsposeMetadataObject> builtInMetadataList = new List<AsposeMetadataObject>();
			Type t = mail.GetType();

			foreach (var prop in t.GetProperties())
			{
				if (prop.CanWrite && IsAllowedType(prop.PropertyType.Name))
				{
					AsposeMetadataObject metadataObject = new AsposeMetadataObject
					{
						PropertyId = i++,
						Name = prop.Name,
						Value = GetValueString(prop.GetValue(mail)),
						Type = ConvertToViewTypeName(prop.PropertyType.Name)
					};
					builtInMetadataList.Add(metadataObject);
				}
			}

			i = 0;
			List<AsposeMetadataObject> customMetadataList = new List<AsposeMetadataObject>();

			foreach (var pair in MapiHelper.GetCustomProperties(mail))
			{
				var name = pair.Value.Name;
				var prop = pair.Value.Property;
				var value = prop.GetValue();

				AsposeMetadataObject metadataObject = new AsposeMetadataObject
				{
					PropertyId = i++,
					Name = name,
					Value = GetValueString(prop.Data, value, (MapiPropertyType)prop.DataType),
					Type = ConvertToViewTypeName(ConvertMailPropertyType((MapiPropertyType)prop.DataType))
				};
				customMetadataList.Add(metadataObject);
			}

			AsposeMetadataProperties response = new AsposeMetadataProperties
			{
				BuiltInMetadataList = builtInMetadataList,
				CustomMetadataList = customMetadataList
			};

			return response;
		}

		private int ConvertToViewTypeName(string name)
		{
			var index = Types.IndexOf(name);

			if (index == -1)
				return 8;

			return index;
		}

		private bool IsAllowedType(string propertyType)
		{
			return AllowedTypes.Contains(propertyType);
		}

		private string ConvertMailPropertyType(MapiPropertyType type)
		{
			switch (type)
			{
				case MapiPropertyType.PT_BOOLEAN:
					return "Boolean";
				case MapiPropertyType.PT_DOUBLE:
					return "Double";
				case MapiPropertyType.PT_LONG:
					return "Number";
				case MapiPropertyType.PT_SYSTIME:
					return "DateTime";
				case MapiPropertyType.PT_UNICODE:
					return "String";
				default:
					return type.ToString();
			}
		}

		private string GetValueString(byte[] bytesValue, object objValue, MapiPropertyType type)
		{
			string retVal = "";

			if (objValue != null)
			{
				switch (type)
				{
					case MapiPropertyType.PT_LONG:
						retVal = Convert.ToInt64(objValue).ToString();
						break;
					case MapiPropertyType.PT_BOOLEAN:
						retVal = Convert.ToBoolean(objValue).ToString();
						break;
					case MapiPropertyType.PT_DOUBLE:
						retVal = Convert.ToDouble(objValue).ToString("F2");
						break;
					case MapiPropertyType.PT_SYSTIME:
						retVal = Convert.ToDateTime(objValue).ToString();
						break;
					case MapiPropertyType.PT_UNICODE:
						retVal = objValue.ToString();
						break;
					default:
						retVal = Encoding.Unicode.GetString(bytesValue);
						break;
				}
			}

			return retVal;
		}

		private string GetValueString(object objValue)
		{
			string retVal = "";
			if (objValue != null)
			{
				retVal = objValue.ToString();
			}
			return retVal;
		}
	}
}

