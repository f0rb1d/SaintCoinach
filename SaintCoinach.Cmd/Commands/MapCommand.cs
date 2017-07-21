﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class MapCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public MapCommand(ARealmReversed realm)
            : base("maps", "Export all map images.") {
            _Realm = realm;
        }

        public override async Task<bool> InvokeAsync(string paramList) {
            var format = ImageFormat.Png;

            if (!string.IsNullOrEmpty(paramList)) {
                var parameters = paramList.Split(' ');
                if (parameters.Contains("jpg"))
                    format = ImageFormat.Jpeg;
                else if (parameters.Contains("png"))
                    format = ImageFormat.Png;
                else
                    OutputError("Invalid map format " + paramList);
            }

            var c = 0;
            var allMaps = _Realm.GameData.GetSheet<SaintCoinach.Xiv.Map>()
                .Where(m => m.TerritoryType != null && m.TerritoryType.Key != 0);

            foreach(var map in allMaps) {
                var img = map.MediumImage;
                if (img == null)
                    continue;

                var outPathSb = new StringBuilder("ui/map/");
                var territoryName = map.TerritoryType.Name.ToString();
                if(territoryName.Length < 3) {
                    outPathSb.AppendFormat("{0}/", territoryName);
                } else {
                    outPathSb.AppendFormat("{0}/", territoryName.Substring(0, 3));
                }

                outPathSb.AppendFormat("{0} - ", territoryName);
                outPathSb.AppendFormat("{0}", ToPathSafeString(map.PlaceName.Name.ToString()));
                if (map.LocationPlaceName != null && map.LocationPlaceName.Key != 0 && !map.LocationPlaceName.Name.IsEmpty)
                    outPathSb.AppendFormat(" - {0}", ToPathSafeString(map.LocationPlaceName.Name.ToString()));
                outPathSb.Append(FormatToExtension(format));

                var outFile = new FileInfo(Path.Combine(_Realm.GameVersion, outPathSb.ToString()));
                if (!outFile.Directory.Exists)
                    outFile.Directory.Create();

                img.Save(outFile.FullName, format);
                ++c;
            }
            OutputInformation("{0} maps saved", c);

            return true;
        }
        
        static string FormatToExtension(ImageFormat format) {
            if (format == ImageFormat.Png)
                return ".png";
            if (format == ImageFormat.Jpeg)
                return ".jpg";

            throw new NotImplementedException();
        }

        static string ToPathSafeString(string input, char invalidReplacement = '_') {
            var sb = new StringBuilder(input);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
                sb.Replace(c, invalidReplacement);
            return sb.ToString();
        }
    }
}
