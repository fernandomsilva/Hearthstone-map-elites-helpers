import csv

def parse_card_list(filename):
	input_file = csv.DictReader(open(filename, 'rt', encoding='latin1'), delimiter=';')
	parsed_list = []

	for row in input_file:
		parsed_list.append({'name': row['Name'], 'rarity': row['Rarity'] if row['Rarity'] == 'Legendary' else 'Common', 'class': row['Class'].lstrip(), 'mana': int(row['Cost'])})

	return parsed_list

def query_list(card_list, field, value):
	result = []
	
	for entry in card_list:
		if entry[field] == value:
			result.append(entry)
	
	return result

#test = parse_card_list("basic-set.csv")
#print(len(test))
#print(len(query_list(test, 'class', 'Any')))

#import itertools

#combs = itertools.combinations(range(10), 3)

#print(len(list(combs)))