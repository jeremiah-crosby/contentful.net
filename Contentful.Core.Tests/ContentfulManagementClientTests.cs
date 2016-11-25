﻿using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Contentful.Core.Tests
{
    public class ContentfulManagementClientTests
    {
        private ContentfulManagementClient _client;
        private FakeMessageHandler _handler;
        private HttpClient _httpClient;
        public ContentfulManagementClientTests()
        {
            _handler = new FakeMessageHandler();
            _httpClient = new HttpClient(_handler);
            _client = new ContentfulManagementClient(_httpClient, new ContentfulOptions()
            {
                DeliveryApiKey = "123",
                ManagementApiKey = "564",
                SpaceId = "666",
                UsePreviewApi = false
            });
        }

        [Fact]
        public void CreatingManagementClientShouldSetHeadersCorrectly()
        {
            //Arrange
            
            //Act
            //Assert
            Assert.Equal("564", _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }
        
        [Fact]
        public async Task CreateContentTypeShouldSerializeRequestCorrectly()
        {
            //Arrange
            _handler.Response = new HttpResponseMessage() {
                Content = new StringContent("{}")
            };
            var contentType = new ContentType();
            contentType.SystemProperties = new SystemProperties();
            contentType.SystemProperties.Id = "123";
            contentType.Name = "Name";
            contentType.Fields = new List<Field>()
            {
                new Field()
                {
                    Name = "Field1",
                    Id = "field1",
                    @Type = "Symbol",
                    Required = true,
                    Localized = false,
                    Disabled = false,
                    Omitted = false
                },
                new Field()
                {
                    Name = "Field2",
                    Id = "field2",
                    @Type = "Location",
                    Required = false,
                    Localized = true,
                    Disabled = false,
                    Omitted = false
                },
                new Field()
                {
                    Name = "Field3",
                    Id = "field3",
                    @Type = "Text",
                    Required = false,
                    Localized = false,
                    Disabled = true,
                    Omitted = false,
                    Validations = new List<IFieldValidator>()
                    {
                        new SizeValidator(3,100)
                    }
                },
                new Field()
                {
                    Name = "Field4",
                    Id = "field4",
                    @Type = "Link",
                    Required = false,
                    Localized = false,
                    Disabled = false,
                    Omitted = false,
                    LinkType = "Asset"
                }
            };


            //Act
            var res = await _client.CreateOrUpdateContentTypeAsync(contentType);

            //Assert
            Assert.Null(res.Name);
        }

        [Fact]
        public async Task EditorInterfaceShouldDeserializeCorrectly()
        {
            //Arrange
            _handler.Response = GetResponseFromFile(@"JsonFiles\EditorInterface.json");

            //Act
            var res = await _client.GetEditorInterfaceAsync("someid");

            //Assert
            Assert.Equal(7, res.Controls.Count);
            Assert.IsType<BooleanEditorInterfaceControlSettings>(res.Controls[4].Settings);
        }

        [Fact]
        public async Task EditorInterfaceShouldSerializeCorrectly()
        {
            //Arrange
            var editorInterface = new EditorInterface();
            editorInterface.Controls = new List<EditorInterfaceControl>()
            {
                new EditorInterfaceControl()
                {
                    FieldId = "field1",
                    WidgetId = SystemWidgetIds.SingleLine
                },
                new EditorInterfaceControl()
                {
                    FieldId = "field2",
                    WidgetId = SystemWidgetIds.Boolean,
                    Settings = new BooleanEditorInterfaceControlSettings()
                    {
                        HelpText = "Help me here!",
                        TrueLabel = "Truthy",
                        FalseLabel = "Falsy"
                    }
                }
            };
            _handler.Response = GetResponseFromFile(@"JsonFiles\EditorInterface.json");

            //Act
            var res = await _client.UpdateEditorInterfaceAsync(editorInterface, "123", 1);

            //Assert
            Assert.Equal(7, res.Controls.Count);
            Assert.IsType<BooleanEditorInterfaceControlSettings>(res.Controls[4].Settings);
        }

        [Fact]
        public async Task WebHookCallDetailsShouldDeserializeCorrectly()
        {
            //Arrange
            _handler.Response = GetResponseFromFile(@"JsonFiles\WebhookCallDetails.json");
            //Act
            var res = await _client.GetWebHookCallDetailsAsync("b", "s");

            //Assert
            Assert.Equal("unarchive", res.EventType);
            Assert.Equal("close", res.Response.Headers["connection"]);

        }

        private HttpResponseMessage GetResponseFromFile(string file)
        {
            //So, this is an ugly hack... Any better way to get the absolute path of the test project?
            var projectPath = Directory.GetParent(typeof(Asset).GetTypeInfo().Assembly.Location).Parent.Parent.Parent.FullName;
            var response = new HttpResponseMessage();
            var fullPath = Path.Combine(projectPath, file);
            response.Content = new StringContent(System.IO.File.ReadAllText(fullPath));
            return response;
        }
    }
}
