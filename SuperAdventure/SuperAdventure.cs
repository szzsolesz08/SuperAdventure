using System.IO;
using Engine.Creatures;
using Engine.Items;
using Engine.Misc;
using Engine.Quests;
using Engine.World;

namespace SuperAdventure {
    public partial class SuperAdventure : Form {
        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure() {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME)) {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else {
                _player = Player.CreateDefaultPlayer();
            }

            MoveTo(_player.CurrentLocation);

            UpdatePlayerStats();
        }

        private void btnNorth_Click(object sender, EventArgs e) {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e) {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e) {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e) {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation) {
            //Does the location have any required items
            if (!_player.HasRequiredItemToEnter(newLocation)) {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                return;
            }

            // Update the player's current location
            _player.CurrentLocation = newLocation;

            // Show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            UpdatePlayerStats();

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null) {
                // See if the player already has the quest, and if they've completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                // See if the player already has the quest
                if (playerAlreadyHasQuest) {
                    // If the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest) {
                        // See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItem(newLocation.QuestAvailableHere);

                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest) {
                            // Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the '" + newLocation.QuestAvailableHere.Name + "' quest." + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            UpdatePlayerStats();
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else {
                    // The player does not already have the quest

                    // Display the messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    newLocation.QuestAvailableHere.QuestCompletionItems.ForEach(qci => {
                        if (qci.Quantity == 1) {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    });
                    rtbMessages.Text += Environment.NewLine;

                    // Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null) {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable) {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();
            ScrollToBottomOfMessages();
        }

        private void UpdateInventoryListInUI() {
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";
            dgvInventory.Rows.Clear();

            _player.Inventory.ForEach(inventoryItem => {
                if (inventoryItem.Quantity > 0) {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            });
        }

        private void UpdateQuestListInUI() {
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";
            dgvQuests.Rows.Clear();

            _player.Quests.ForEach(playerQuest => {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            });
        }

        private void UpdateWeaponListInUI() {
            List<Weapon> weapons = new List<Weapon>();

            _player.Inventory.ForEach(inventoryItem => {
                if (inventoryItem.Details is Weapon) {
                    if (inventoryItem.Quantity > 0) {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            });

            if (weapons.Count == 0) {
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";
                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI() {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            _player.Inventory.ForEach(inventoryItem => {
                if (inventoryItem.Details is HealingPotion) {
                    if (inventoryItem.Quantity > 0) {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            });

            if (healingPotions.Count == 0) {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e) {
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            _currentMonster.CurrentHitPoints -= damageToMonster;

            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;

            if (_currentMonster.CurrentHitPoints <= 0) {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                UpdatePlayerStats();
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;

                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine;

                List<InventoryItem> lootedItems = new List<InventoryItem>();
                _currentMonster.LootTable.ForEach(lootItem => {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage) {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                });

                if (lootedItems.Count == 0) {
                    _currentMonster.LootTable.ForEach(lootItem => {
                        if (lootItem.IsDefaultItem) {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    });
                }

                lootedItems.ForEach(inventoryItem => {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1) {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name + Environment.NewLine;
                    }
                    else {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + Environment.NewLine;
                    }
                });

                UpdatePlayerStats();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                rtbMessages.Text += Environment.NewLine;

                MoveTo(_player.CurrentLocation);
            }
            else {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);
                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
                _player.CurrentHitPoints -= damageToPlayer;
                UpdatePlayerStats();

                if (_player.CurrentHitPoints <= 0) {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
            ScrollToBottomOfMessages();
        }

        private void btnUsePotion_Click(object sender, EventArgs e) {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);

            if (_player.CurrentHitPoints > _player.MaximumHitPoints) {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.Inventory.ForEach(inventoryItem => {
                if (inventoryItem.Details.ID == potion.ID) {
                    inventoryItem.Quantity--;
                }
            });

            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0) {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            UpdatePlayerStats();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
            ScrollToBottomOfMessages();
        }

        private void UpdatePlayerStats() {
            // Refresh player information and inventory controls
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void ScrollToBottomOfMessages() {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e) {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show("This action will delete your saved game.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes) {
                if (File.Exists(PLAYER_DATA_FILE_NAME)) {
                    File.Delete(PLAYER_DATA_FILE_NAME);
                    MessageBox.Show("Your saved game has been deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _player = Player.CreateDefaultPlayer();

                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));

                    UpdatePlayerStats();
                    UpdateInventoryListInUI();
                    UpdateQuestListInUI();
                    UpdateWeaponListInUI();
                    UpdatePotionListInUI();

                    rtbMessages.Text = "";
                }
                else {
                    MessageBox.Show("No saved game found to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
