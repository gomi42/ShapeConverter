using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeConverter.BusinessLogic.Generators
{
    public static class GeometrySourceGenerator
    {
        /// <summary>
        /// Generate the XAML source code (a <Path/> for a single graphic path
        /// </summary>
        public static string GenerateGeometry(GraphicVisual visual)
        {
            var tag = "Geometry";
            StringBuilder result = new StringBuilder();

            var geometries = StreamSourceGenerator.GenerateStreamGeometries(visual);
            int i = 1;

            foreach (var geometry in geometries)
            {
                result.Append($"<{tag} x:Key=\"shape{i}\">");
                result.Append(geometry);
                result.AppendLine($"</{tag}>");
                i++;
            }

            return result.ToString();
        }
    }
}
