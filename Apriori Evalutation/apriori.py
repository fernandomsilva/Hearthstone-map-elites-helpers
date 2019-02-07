def calculate_support_count(list_of_items, decksets):
	support_count = {}
	for item in list_of_items:
		support_count[item] = 0

	size_of_itemset = len(list_of_items[0])
		
	for key in support_count:
		for deckset in decksets:
			if len(set(key).intersection(deckset)) == size_of_itemset:
				support_count[key] = support_count[key] + 1
	
	list_of_keys_to_delete = []
	for key in support_count:
		if support_count[key] == 0:
			list_of_keys_to_delete.append(key)
	
	for key in list_of_keys_to_delete:
		del support_count[key]
	
	return support_count
	
def apriori(T, epsilon):
	result = []

	set_of_cards = set()
	
	for deck in T:
		set_of_cards = set_of_cards.union(deck)
	
	list_of_k_tuples = []
	for card in sorted(set_of_cards):
		list_of_k_tuples.append(tuple([card]))
		
	C_1 = calculate_support_count(list_of_k_tuples, T)
	L_1 = {}
	
	for key in C_1:
		if C_1[key] >= epsilon:
			L_1[key] = C_1[key]
			
	list_of_itemsets = sorted(list(L_1.keys()))
	
	for key in L_1.keys():
		result.append((key, L_1[key]))
	
	L_k = L_1
	k = 2
	while True:
		list_of_k_tuples = []
		num_of_elements_in_common = k - 2
		
		for x in range(0, len(list_of_itemsets)-1):
			for y in range(x+1, len(list_of_itemsets)):
				if len(set(list_of_itemsets[x]).intersection(set(list_of_itemsets[y]))) >= num_of_elements_in_common:
					list_of_k_tuples.append(tuple(sorted(set(list_of_itemsets[x]).union(set(list_of_itemsets[y])))))
		
		if len(list_of_k_tuples) < 2:
			#C_k = L_k
			break
		
		C_k = calculate_support_count(list_of_k_tuples, T)
		L_k = {}
		
		for key in C_k:
			if C_k[key] >= epsilon:
				L_k[key] = C_k[key]
		
		if len(L_k) == 0:
			for key in C_k.keys():
				result.append((key, C_k[key]))

			break
		
		for key in L_k.keys():
			result.append((key, L_k[key]))

		list_of_itemsets = sorted(list(L_k.keys()))
		k = k + 1
		#for itemset in list_of_itemsets:
		
	return result
			
