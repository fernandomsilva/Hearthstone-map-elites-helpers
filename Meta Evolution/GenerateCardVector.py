import csv

def load_decks_set(filename):
	input_file = csv.reader(open(filename), delimiter=';')
	
	set_of_cards = set()
	
	for row in input_file:
		set_of_cards = set_of_cards.union(set(row[3].split('*')))
	
	return sorted(set_of_cards)

def load_card_data(filename):
	input_file = csv.DictReader(open(filename, 'rt', encoding='latin1'), delimiter=';')
	parsed_list = []

	for row in input_file:
		parsed_list.append({'name': row['Name'], 'type': row['Type'], 'mana': row['Cost'], 'atk': row['Atk'], 'hp': row['HP']})

	return parsed_list

def save_card_data_list(output_filepath, card_data):
	output_file = open(output_filepath, 'w')
	
	for card, info in card_data:
		output_file.write(card + ";" + str(info[0]) + ";" + str(info[1]) + ";" + str(info[2]) + "\n")
	
	output_file.close()

def generate_card_vector(output_filepath):
	full_list = load_card_data("basic-set.csv") + load_card_data("classic-set.csv")
	dict_data = {}
	
	sorted_deck_card_list = load_decks_set("player_decks.csv")
	result_card_data = []
	result_vector = []
	
	for card in full_list:
		if card['name'] in sorted_deck_card_list:
			dict_data[card['name']] = {'type': card['type'], 'mana': card['mana'], 'atk': card['atk'], 'hp': card['hp']}
	
	for card in sorted_deck_card_list:
		if dict_data[card]['type'] == "Spell":
			result_card_data.append((card, (int(dict_data[card]['mana']),'','')))
			result_vector.append(0)
		else:
			result_card_data.append((card, (int(dict_data[card]['mana']), int(dict_data[card]['atk']), int(dict_data[card]['hp']))))
			result_vector.extend([0, 0, 0])
	
	save_card_data_list(output_filepath, result_card_data)
	
	return result_vector

#vector = generate_card_vector("test.csv")

#print(len(vector))
