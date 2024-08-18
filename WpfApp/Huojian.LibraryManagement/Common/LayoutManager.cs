using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Huojian.LibraryManagement.Extentions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShadowBot.Common.Utilities;
using Formatting = System.Xml.Formatting;

namespace Huojian.LibraryManagement.Common
{
    public class LayoutManager : INotifyPropertyChanged
    {
        private readonly string _fileName;

        public LayoutManager(string fileName)
        {
            _fileName = fileName;
            if (File.Exists(_fileName))
            {
                try
                {
                    var jObject = JObjectHelper.FromFile(_fileName);
                    Left = jObject.Resolve<EditorLayoutItem>("left");
                    Right = jObject.Resolve<EditorLayoutItem>("right");
                    Bottom = jObject.Resolve<EditorLayoutItem>("bottom");
                }
                catch (Exception ex)
                {
                    Left = new EditorLayoutItem { IsExpanded = true, Size = 260 };
                    Right = new EditorLayoutItem { IsExpanded = true, Size = 260 };
                    Bottom = new EditorLayoutItem { IsExpanded = true, Size = 230 };
                }
            }
            else
            {
                Left = new EditorLayoutItem { IsExpanded = true, Size = 260 };
                Right = new EditorLayoutItem { IsExpanded = true, Size = 260 };
                Bottom = new EditorLayoutItem { IsExpanded = true, Size = 230 };
            }
        }

        public EditorLayoutItem Left { get; set; }

        public EditorLayoutItem Bottom { get; set; }

        public EditorLayoutItem Right { get; set; }

        public void Save()
        {
            JObjectHelper.SaveToFile(new
            {
                left = Left,
                bottom = Bottom,
                right = Right
            }, _fileName);
        }

        public JToken GetCurrentLayout()
        {
            var layout = new
            {
                left = Left,
                bottom = Bottom,
                right = Right
            };

            var jsonContent = JsonConvert.SerializeObject(layout, Newtonsoft.Json.Formatting.Indented);
            return JToken.Parse(jsonContent);
        }

        public void ReloadLayout(JToken token)
        {
            if (token == null)
                return;
            Left = token.Resolve<EditorLayoutItem>("left");
            Right = token.Resolve<EditorLayoutItem>("right");
            Bottom = token.Resolve<EditorLayoutItem>("bottom");
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class EditorLayoutItem : INotifyPropertyChanged
    {
        [JsonProperty("size")]
        public double Size { get; set; }

        [JsonProperty("isExpanded")]
        public bool IsExpanded { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
