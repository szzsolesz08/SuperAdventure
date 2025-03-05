using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Engine.Items;
using Engine.Quests;
using Engine.World;

namespace Engine.Creatures {
    public class Player : LivingCreature {
        public int Gold { get; set; }
        public int ExperiencePoints { get; private set; }
        public int Level { get { return ((ExperiencePoints / 100) + 1); } }
        public Location CurrentLocation { get; set; }
        public Weapon CurrentWeapon { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints) {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer() {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.World.ItemByID(World.World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.World.LocationByID(World.World.LOCATION_ID_HOME);
            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData) {
            try {
                XmlDocument playerData = new XmlDocument();
                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);
                player.CurrentLocation = World.World.LocationByID(Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText));

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null) {
                    player.CurrentWeapon = (Weapon)World.World.ItemByID(Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText));
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/Inventory/InventoryItem")) {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);
                    for (int i = 0; i < quantity; i++) {
                        player.AddItemToInventory(World.World.ItemByID(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/Quests/PlayerQuest")) {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);
                    PlayerQuest playerQuest = new PlayerQuest(World.World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;
                    player.Quests.Add(playerQuest);
                }
                return player;
            }
            catch {

                return Player.CreateDefaultPlayer();
            }
        }

        public void AddExperiencePoints(int experiencePointsToAdd) {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = Level * 10;
        }

        public bool HasRequiredItemToEnter(Location location) {
            if (location.ItemRequiredToEnter == null) {
                return true;
            }

            return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest quest) {
            return Quests.Exists(pq => pq.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest(Quest quest) {
            return Quests.Exists(pq => pq.Details.ID == quest.ID && pq.IsCompleted);
        }

        public bool HasAllQuestCompletionItem(Quest quest) {
            bool hasAllItems = true;
            quest.QuestCompletionItems.ForEach(qci => {
                bool foundItemInPlayersInventory = false;
                Inventory.ForEach(ii => {
                    if (ii.Details.ID == qci.Details.ID) {
                        foundItemInPlayersInventory = true;
                        if (ii.Quantity < qci.Quantity) {
                            hasAllItems = false;
                        }
                    }
                });
                if (!foundItemInPlayersInventory) {
                    hasAllItems = false;
                }
            });
            return hasAllItems;
        }

        public void RemoveQuestCompletionItems(Quest quest) {
            quest.QuestCompletionItems.ForEach(qci => {
                Inventory.ForEach(ii => {
                    if (ii.Details.ID == qci.Details.ID) {
                        ii.Quantity -= qci.Quantity;
                    }
                });
            });
        }

        public void AddItemToInventory(Item itemToAdd) {
            bool alreadyInInventory = false;
            Inventory.ForEach(ii => {
                if (ii.Details.ID == itemToAdd.ID) {
                    alreadyInInventory = true;
                    ii.Quantity++;
                    return;
                }
            });
            if (!alreadyInInventory) {
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
        }

        public void MarkQuestCompleted(Quest quest) {
            PlayerQuest playerQuest = Quests.Find(pq => pq.Details.ID == quest.ID);
            if (playerQuest != null) {
                playerQuest.IsCompleted = true;
            }
        }

        public string ToXmlString() {
            XmlDocument playerData = new XmlDocument();

            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if (CurrentWeapon != null) {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            XmlNode inventory = playerData.CreateElement("Inventory");
            player.AppendChild(inventory);
            this.Inventory.ForEach(i => {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = i.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = i.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);
                inventory.AppendChild(inventoryItem);
            });

            XmlNode quests = playerData.CreateElement("Quests");
            player.AppendChild(quests);
            this.Quests.ForEach(q => {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = q.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = q.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);
                quests.AppendChild(playerQuest);
            });

            return playerData.InnerXml;
        }
    }
}
