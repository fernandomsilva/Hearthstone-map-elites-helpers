from card_list_parse import *
import itertools

class_cards_db = {'Hunter': [], 'Paladin': [], 'Warlock': []}
range_class = {'Hunter': [], 'Paladin': [], 'Warlock': [], 'Legendary_Hunter': 0, 'Legendary_Paladin': 0, 'Legendary_Warlock': 0}

card_db = parse_card_list("basic-set.csv") + parse_card_list("classic-set.csv")

for class_name in class_cards_db.keys():
	class_cards_db[class_name] = query_list(card_db, 'class', 'Any') + query_list(card_db, 'class', class_name)
	class_cards_db[class_name] = sorted(class_cards_db[class_name], key=lambda k: k['rarity'])

	num_of_legendary = len(query_list(class_cards_db[class_name], 'rarity', 'Legendary'))
	num_of_common = len(class_cards_db[class_name]) - num_of_legendary
	
	range_class[class_name] = range((2 * num_of_common) + num_of_legendary)
	range_class["Legendary_" + class_name] = num_of_legendary

#print(len(range_class['Hunter']))
#print(range_class['Legendary_Hunter'])
#decks_db = {'Hunter': [], 'Paladin': [], 'Warlock': []}

decks_db['Hunter'] = itertools.combinations(range_class['Hunter'], 30)
