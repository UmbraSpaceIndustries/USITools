using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace USITools
{
    public class ShipThumbnailService
    {
        private readonly Dictionary<string, string> _fileNameCache
            = new Dictionary<string, string>();

        private Texture2D CreateThumbnail(ShipConstruct ship)
        {
            var folderPath = "thumbs";
            CraftThumbnail.TakeSnaphot(ship, 256, folderPath, ship.shipName);
            var fileName
                = $"{HighLogic.SaveFolder}_{ship.shipFacility}_{ship.shipName}.png";
            var filePath = Path.Combine(KSPUtil.ApplicationRootPath, folderPath, fileName);
            _fileNameCache.Add(ship.shipName, filePath);
            return LoadThumbnail(filePath);
        }

        private string FindExistingThumbnail(ShipConstruct ship)
        {
            var folderPath = Path.Combine(KSPUtil.ApplicationRootPath, "thumbs");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var fileName
                = $"{HighLogic.SaveFolder}_{ship.shipFacility}_{ship.shipName}.png";
            var filePath = Path.Combine(folderPath, fileName);
            return File.Exists(filePath) ? filePath : null;
        }

        public Texture2D GetThumbnail(ShipConstruct ship)
        {
            if (_fileNameCache.ContainsKey(ship.shipName))
            {
                return LoadThumbnail(_fileNameCache[ship.shipName]);
            }
            var filePath = FindExistingThumbnail(ship);
            if (string.IsNullOrEmpty(filePath))
            {
                return CreateThumbnail(ship);
            }
            _fileNameCache.Add(ship.shipName, filePath);
            return LoadThumbnail(filePath);
        }

        private Texture2D LoadThumbnail(string filePath)
        {
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            if (!File.Exists(filePath))
            {
                return texture;
            }
            texture.LoadImage(File.ReadAllBytes(filePath));
            return texture;
        }
    }
}
