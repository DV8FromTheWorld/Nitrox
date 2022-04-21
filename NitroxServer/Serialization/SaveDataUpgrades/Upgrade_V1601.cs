using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public class Upgrade_V1601 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 6, 0, 1);

        protected override void UpgradeWorldData(JObject data)
        {
            List<string> modules = new();
            foreach (JToken moduleEntry in data["InventoryData"]["Modules"])
            {
                JToken itemId = moduleEntry["ItemId"];
                if (modules.Contains(itemId.ToString()))
                {
                    itemId = new NitroxId().ToString();
                    // this line is enough to modify the original data
                    moduleEntry["ItemId"] = itemId;
                }
                modules.Add(itemId.ToString());
            }
        }

        protected override void UpgradeBaseData(JObject data)
        {
            Action<JToken> upgradeRotationMetadata = basePieceList =>
            {
                foreach (JToken basePieceEntry in basePieceList)
                {
                    JToken rotationMetadata = basePieceEntry["RotationMetadata"]["value"];
                    if (rotationMetadata.Type != JTokenType.Null)
                    {
                        rotationMetadata["$type"] = rotationMetadata["$type"].ToString()
                             .Replace("AnchoredFaceRotationMetadata", "AnchoredFaceBuilderMetadata")
                             .Replace("BaseModuleRotationMetadata", "BaseModuleBuilderMetadata")
                             .Replace("CorridorRotationMetadata", "CorridorBuilderMetadata")
                             .Replace("MapRoomRotationMetadata", "MapRoomBuilderMetadata");
                    }
                }
            };

            upgradeRotationMetadata(data["PartiallyConstructedPieces"]);
            upgradeRotationMetadata(data["CompletedBasePieceHistory"]);
        }
    }
}
