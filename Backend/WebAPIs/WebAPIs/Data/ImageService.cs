using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace WebAPIs.Data
{
    public class ImageService
    {
        public string Image(IFormFile img)
        {
            if(img == null)
            {
                return null;
            }
            string content = null;
            using (var target = new MemoryStream())
            {
                img.CopyTo(target);
                var fileContent = target.ToArray();
                content = Convert.ToBase64String(fileContent);
            }
            return content;
        }
    }
}
