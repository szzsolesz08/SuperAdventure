using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine {
    public class Player : LivingCreature {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level { get { return ((ExperiencePoints / 100) + 1); } }
        public Location CurrentLocation { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints) {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
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
    }
}
