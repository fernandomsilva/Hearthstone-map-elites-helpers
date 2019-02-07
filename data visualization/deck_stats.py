from card_list_parse import *
import numpy

def deck_stats(deck_list, card_db):
	mana_list = []

	for card in deck_list:
		mana_list.append(query_list(card_db, "name", card)[0]["mana"])

	avg = float(sum(mana_list)) / float(len(mana_list))
	var = numpy.var(mana_list)
	
	return avg, var