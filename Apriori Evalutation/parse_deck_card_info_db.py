import os
import csv
import json

#experiments = ["ExpSet1", "ExpSet2"]
#experiments = ['ExpSet2']
experiments = ["ExpSet1", "ExpSet2", "Nerfed"]
archetypes = ["Aggro", "Control"]
classes = ["Hunter", "Paladin", "Warlock"]

database_template = {'ExpSet1_deck_count': 0, 'ExpSet1_card_deck_list': {}, 'ExpSet2_deck_count': 0, 'ExpSet2_card_deck_list': {}, 'Nerfed_deck_count': 0, 'Nerfed_card_deck_list': {}}
database = {'Hunter Aggro': dict(database_template), 'Hunter Control': dict(database_template), 'Paladin Aggro': dict(database_template), 'Paladin Control': dict(database_template), 'Warlock Aggro': dict(database_template), 'Warlock Control': dict(database_template)}

output_path = "data/"

paths = []
for exp in experiments:
	for arc in archetypes:
		for cla in classes:
			paths.append("Experiments/" + exp + "/" + arc + "/" + cla + "/")

#path = "Experiments/ExpSet2/Aggro/Warlock/"
#output_file = "apriori_expset2_aggro_warlock.csv"

for path in paths:
	temp = path.split('/')
	output_full_path = output_path #+ "epsilon " + str(epsilon_factor) + "/"
	if not os.path.exists(output_full_path):
		os.makedirs(output_full_path)
	output_file = output_full_path + "deck_card_db.json"

	elite_map_log_file = open(path + "elite_map_log.csv")
	individuals_log_file = csv.DictReader(open(path + "individual_log.csv"))

	for line in elite_map_log_file:
		pass

	elite_map_log_file.close()

	data_list = line.split(',')
	deck_ids = []

	for entry in data_list:
		if ':' in entry:
			deck_ids.append((entry.split(":"))[3])

	deck_dict = {}

	for row in individuals_log_file:
		deck_dict[row["Individual"]] = row["Deck"].split('*')

	total_decks = len(deck_ids)
	card_deck_dict = {}	

	for id in deck_ids:
		deck = set(deck_dict[id])
		for card in deck:
			if card not in card_deck_dict:
				card_deck_dict[card] = [id]
			else:
				card_deck_dict[card].append(id)
	
	experiment = temp[1]
	archetype = temp[2]
	char_class = temp[3]
	
	database[char_class + ' ' + archetype][experiment + '_deck_count'] = total_decks
	database[char_class + ' ' + archetype][experiment + '_card_deck_list'] = dict(card_deck_dict)
	
	print(path, " done!")

output_file = open(output_file, 'w')
json.dump(database, output_file)
output_file.close()