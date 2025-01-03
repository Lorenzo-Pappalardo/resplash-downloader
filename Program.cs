using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var httpClient = new HttpClient() { };

app.MapGet("/", () => "Hello World!");

app.MapPost("/download", async (string[] urls) =>
{
  var regex = new Regex(@"^https:\/\/images\.unsplash\.com\/photo-\w+-\w+\?");
  var filtered = urls.Where(url => regex.Match(url).Success);

  try
  {
    var photosByteArrays = await Task.WhenAll(filtered.Select(async url => await httpClient.GetByteArrayAsync(url)));
    var randomName = RandomNumberGenerator.GetHexString(10);
    var basePath = $"downloaded/{randomName}";

    if (photosByteArrays != null)
    {
      for (int i = 0; i < photosByteArrays.Length; i++)
      {
        if (photosByteArrays[i] != null)
        {
          Directory.CreateDirectory(basePath);

          await File.WriteAllBytesAsync($"{basePath}/photo{i}.jpeg", photosByteArrays[i]);
        }
      }

      var fastZip = new FastZip();
      fastZip.CreateZip($"{basePath}.zip", $"{basePath}/", true, null);

      Directory.Delete(basePath, true);
    }
  }
  catch { }
});

app.Run();

/*
Example request body:

[
  "https://images.unsplash.com/photo-1730119986244-eb33b57b3950?ixid=M3w2NzYxNTh8MHwxfGFsbHwxfHx8fHx8fHwxNzM1ODUwMjc0fA&ixlib=rb-4.0.3",
  "https://images.unsplash.com/photo-1729877251622-a9043a2ec183?ixid=M3w2NzYxNTh8MHwxfGFsbHwyfHx8fHx8fHwxNzM1ODUwMjc0fA&ixlib=rb-4.0.3",
  "https://images.unsplash.com/photo-1723654864018-36cc407e4a69?ixid=M3w2NzYxNTh8MHwxfGFsbHwzfHx8fHx8fHwxNzM1ODUwMjc0fA&ixlib=rb-4.0.3",
  "https://images.unsplash.com/photo-1731412924028-204b15ca8f1d?ixid=M3w2NzYxNTh8MHwxfGFsbHw4fHx8fHx8fHwxNzM1ODUwMjc0fA&ixlib=rb-4.0.3",
  "https://images.unsplash.com/photo-1731450453063-31716e7b71af?ixid=M3w2NzYxNTh8MHwxfGFsbHw5fHx8fHx8fHwxNzM1ODUwMjc0fA&ixlib=rb-4.0.3"
]
*/