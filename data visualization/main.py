import os
import csv
from card_list_parse import *
from deck_stats import *

#experiments = ["ExpSet1", "ExpSet2"]
#experiments = ['ExpSet2']
experiments = ["ExpSet1", "ExpSet2", "Nerfed"]
archetypes = ["Aggro", "Control"]
classes = ["Hunter", "Paladin", "Warlock"]

output_path = "data/"

paths = []
for exp in experiments:
	for arc in archetypes:
		for cla in classes:
			paths.append("Experiments/" + exp + "/" + arc + "/" + cla + "/")

#path = "Experiments/ExpSet2/Aggro/Warlock/"
#output_file = "apriori_expset2_aggro_warlock.csv"

card_db = parse_card_list("basic-set.csv") + parse_card_list("classic-set.csv")

#print(query_list(card_db, "mana", 7))

for path in paths:
	temp = path.split('/')
	output_full_path = output_path
	if not os.path.exists(output_full_path):
		os.makedirs(output_full_path)
	output_file = output_full_path + "heatmapdata_" + temp[1].lower() + "_" + temp[2].lower() + "_" + temp[3].lower() + ".csv"

	elite_map_log_file = open(path + "elite_map_log.csv")
	individuals_log_file = csv.DictReader(open(path + "individual_log.csv"))

	for line in elite_map_log_file:
		pass

	elite_map_log_file.close()

	data_list = line.split(',')
	deck_ids = []
	deck_fitness = {}

	for entry in data_list:
		if ':' in entry:
			deck_ids.append((entry.split(":"))[3])
			deck_fitness[(entry.split(":"))[3]] = int((entry.split(":"))[5])

	deck_dict = {}

	for row in individuals_log_file:
		deck_dict[row["Individual"]] = row["Deck"].split('*')
	
	output_data = []
	
	for id in deck_ids:
		average, variance = deck_stats(deck_dict[id], card_db)
		output_data.append([id, deck_fitness[id], average, variance])

	output_file = open(output_file, 'w')
	output_file.write("deck_id;fitness;average;variance;\n")
	for item in output_data:
		output_file.write(str(item[0]) + ";" + str(item[1]) + ";" + str(item[2]) + ";" + str(item[3]) + ";\n")
	output_file.close()
	
	print(path, " done!")
		
	#print(deck_fitness)
	'''
	list_of_set_of_elite_decks = []

	for id in deck_ids:
		list_of_set_of_elite_decks.append(set(deck_dict[id]))
	
	epsilon = len(list_of_set_of_elite_decks) * epsilon_factor
	result = apriori(list_of_set_of_elite_decks, epsilon)

	percent_result = {}
	for item in result:
		percent_result[item[0]] = float(item[1]) / float(len(list_of_set_of_elite_decks))

	output_file = open(output_file, 'w')
	for item in percent_result:
		output_file.write(str(item) + ";" + str(percent_result[item]) + "\n")
	output_file.close()
	
	print(path, " done!")
	'''