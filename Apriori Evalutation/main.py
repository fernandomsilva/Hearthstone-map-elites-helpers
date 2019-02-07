import os
import csv
from apriori import *

#experiments = ["ExpSet1", "ExpSet2"]
#experiments = ['ExpSet2']
experiments = ['Nerfed']
archetypes = ["Aggro", "Control"]
classes = ["Hunter", "Paladin", "Warlock"]

output_path = "data/"
epsilon_factor = 0.25

paths = []
for exp in experiments:
	for arc in archetypes:
		for cla in classes:
			paths.append("Experiments/" + exp + "/" + arc + "/" + cla + "/")

#path = "Experiments/ExpSet2/Aggro/Warlock/"
#output_file = "apriori_expset2_aggro_warlock.csv"

for path in paths:
	temp = path.split('/')
	output_full_path = output_path + "epsilon " + str(epsilon_factor) + "/"
	if not os.path.exists(output_full_path):
		os.makedirs(output_full_path)
	output_file = output_full_path + "apriori_" + temp[1].lower() + "_" + temp[2].lower() + "_" + temp[3].lower() + ".csv"

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