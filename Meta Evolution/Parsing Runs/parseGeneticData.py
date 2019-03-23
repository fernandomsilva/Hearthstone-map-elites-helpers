import sys, os
import numpy as np

folder = sys.argv[1]

i = 0

results = []

default_card_stats = {}
default_card_stats_list = []
file_data = open(folder + "/default_card_stats.csv", 'r')
for line in file_data:
	data = line.split(';')
	atk = -1
	if (len(data[2]) > 0):
		atk = int(data[2])
	hp = -1
	if (len(data[3]) > 0) and data[3] != '\n':
		hp = int(data[3])
	default_card_stats[data[0]] = {'mana': int(data[1]), 'atk': atk, 'hp': hp}
	default_card_stats_list.append([data[0], int(data[1]), atk, hp])
file_data.close()

def parseVectorStrToInt(vector_str):
	temp = vector_str[1:-1].split(',')
	
	return [int(x) for x in temp]

def calculateVectorWeight(vector, card_stats):
	result = 0
	index = 0
	
	for card, mana, atk, hp in card_stats:
		result += abs(vector[index]) * 2
		index += 1
	
		if atk >= 0:
			result += abs(vector[index])
			result += abs(vector[index+1])
			
			index += 2
	
	return result
	
def listOfChangedCards(vector, card_stats):
	result = []
	
	index = 0
	for card, mana, atk, hp in card_stats:
		if vector[index] != 0:
			result.append(card)

			if atk >= 0:
				index += 2				
		
		else:
			if atk >= 0:
				if vector[index+1] != 0 or vector[index+2] != 0:
					result.append(card)
				
				index += 2

		index += 1
	
	return result

def calculateDirectedWeightPerDeck(vector, card_stats, deck_list):
	result = np.zeros(3)  #This represents how much, and in which direction, the nerfs and buffs affected each of the three decks
	#TODO: replace the constant 3 for a variable containig the number of decks
	
	list_of_changed_cards = listOfChangedCards(vector, card_stats)
	
	index = 0
	
	for card, mana, atk, hp in card_stats:
		manaChange = vector[index] * 2
		index += 1
		
		statsChange = 0
	
		affectedDecks = deck_list.count(card) * (1 if card in list_of_changed_cards else 0)  #Array that indicates if the deck under consideration uses that card
	
		if atk >= 0:
		#A positive stat change represents a buff, where a positive mana change represents a nerf, hence the negative sign
			statsChange += -vector[index] 
			statsChange += -vector[index+1]
			
			index += 2
	
		result = result + (manaChange + statsChange) * affectedDecks
	
	return result
	
def extractDeckListFromFile(filepath):
	result = []

	file_data = open(filepath, 'r')
	
	for entry in file_data:
		deck = entry.split(';')[3]
		deck = deck.split('*')
		
		result.append({'name': entry.split(';')[0], 'deck': deck})
	
	file_data.close()
	
	return result
	
def calculateDeckNerfInfluence(vector, card_stats, deck_list):
	result = 0
	index = 0
	
	for card, mana, atk, hp in card_stats:
		if card in deck_list:
			if vector[index] != 0:
				result += deck_list.count(card)

				if atk >= 0:
					index += 2				
			
			else:
				if atk >= 0:
					if vector[index+1] != 0 or vector[index+2] != 0:
						result += deck_list.count(card)
					
					index += 2

			index += 1
	
	return result

def outputDetailedNerfs(filepath, individual):
	file_data = open(filepath, 'w')
	
	index = 0
	for card, mana, atk, hp in default_card_stats_list:
		temp = card + ';'
		
		temp_mana = mana + individual[index]
		if temp_mana < 0:
			temp_mana = 0
		elif temp_mana > 10:
			temp_mana = 10
		
		temp += str(temp_mana) + ';'
		
		index += 1
		
		if atk >= 0:
			temp_atk = atk + individual[index]
			temp_hp = hp + individual[index+1]
			if temp_atk < 0:
				temp_atk = 0
			if temp_hp < 0:
				temp_hp = 0
			temp += str(temp_atk) + ';'
			temp += str(temp_hp)
			
			index += 2
		else:
			temp += ';'
		
		file_data.write(temp + '\n')
	
	file_data.close()
	
while True:
	if os.path.isdir(folder + "/gen" + str(i)):
		filepath = folder + "/gen" + str(i) + "/output.txt"
		if os.path.isfile(filepath):
			file_data = open(filepath)
			
			for line in file_data:
				data = line.replace('],', '];')
				data = data[1:-1].split(";")
				
				vector = parseVectorStrToInt(data[0])
				
				results.append([vector, float(data[1].split(')')[0]), i, calculateVectorWeight(vector, default_card_stats_list)])
			
			file_data.close()
			
			i += 1
		else:
			break
	else:
		break
		
results = sorted(results, key=lambda k: k[1])

output_file = open(folder + "/parsed.csv", 'w')
for entry in results:
	output_file.write(str(entry[0]) + ';' + str(entry[1]) + ';' + str(entry[2]) + ';' + str(entry[3]) + '\n')
output_file.close()

outputDetailedNerfs(folder + "/topNerf.csv", results[0][0])

deck_influence_file = open(folder + "/deckTopNerfInfluence.py", 'w')

deck_data = extractDeckListFromFile(folder + "/player_decks.csv")
for deck in deck_data:
	influence = calculateDirectedWeightPerDeck(results[0][0], default_card_stats_list, deck['deck'])
	deck_influence_file.write(deck['name'] + ";" + str(influence) + '\n')
	
deck_influence_file.close()
