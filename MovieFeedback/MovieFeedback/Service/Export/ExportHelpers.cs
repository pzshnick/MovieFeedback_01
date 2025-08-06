using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using MovieFeedback.ViewModels;

namespace MovieFeedback.Service.Export
{

    namespace MovieFeedback.Helpers
    {
        public static class ExportHelpers
        {
            public static byte[] GenerateCsv(List<FavoriteMovieViewModel> favorites)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Title,Rating,ReleaseDate,Genres");

                foreach (var fav in favorites)
                {
                    var genres = string.Join(";", fav.Genres);
                    sb.AppendLine($"\"{fav.Title}\",{fav.Rating},{fav.ReleaseDate},{genres}");
                }

                return Encoding.UTF8.GetBytes(sb.ToString());
            }

            public static byte[] GenerateXml(List<FavoriteMovieViewModel> favorites)
            {
                var serializer = new XmlSerializer(typeof(List<FavoriteMovieViewModel>));
                using var ms = new MemoryStream();
                serializer.Serialize(ms, favorites);
                return ms.ToArray();
            }

            public static byte[] GenerateJson(List<FavoriteMovieViewModel> favorites)
            {
                var json = JsonSerializer.Serialize(favorites, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return Encoding.UTF8.GetBytes(json);
            }

            public static byte[] GenerateRdf(List<FavoriteMovieViewModel> favorites)
            {
                var sb = new StringBuilder();
                sb.AppendLine("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">");

                foreach (var fav in favorites)
                {
                    sb.AppendLine($"  <rdf:Description rdf:about=\"http://example.com/movie/{fav.MovieId}\">");
                    sb.AppendLine($"    <title>{System.Security.SecurityElement.Escape(fav.Title)}</title>");
                    sb.AppendLine($"    <rating>{fav.Rating}</rating>");
                    sb.AppendLine($"    <releaseDate>{fav.ReleaseDate}</releaseDate>");
                    sb.AppendLine($"    <genres>{System.Security.SecurityElement.Escape(string.Join(", ", fav.Genres))}</genres>");
                    sb.AppendLine($"  </rdf:Description>");
                }

                sb.AppendLine("</rdf:RDF>");

                return Encoding.UTF8.GetBytes(sb.ToString());
            }
        }
    }

}
