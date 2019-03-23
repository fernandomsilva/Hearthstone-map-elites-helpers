import random, os, sys, time, math, functools

import GenerateCardVector as gcv
import parseWorkerFiles as pwf

from deap import base
from deap import creator
from deap import tools

range_of_variable = 3
#size_of_individual = 5
num_of_individuals = 3

def make_dir(dir):
	if not os.path.exists(dir):
		os.makedirs(dir)

directory = sys.argv[1]
number_of_workers = int(sys.argv[2])
number_of_decks = int(sys.argv[3])
make_dir(directory)

default_vector = gcv.generate_card_vector(directory + "/default_card_stats.csv")

size_of_individual = len(default_vector)

def saveToFile(filepath, input_str_list):
	directory_str_list = filepath.split('/')
	new_directory = ""
	for dir in directory_str_list[:-1]:
		new_directory = new_directory + dir + "/"
		make_dir(new_directory)

	make_dir(new_directory + "worker_data")
		
	file_data = open(filepath, 'a')
	for line in input_str_list:
		file_data.write(str(line) + "\n")
	file_data.close()

creator.create("FitnessMin", base.Fitness, weights=(-1.0,))
creator.create("Individual", list, fitness=creator.FitnessMin)

toolbox = base.Toolbox()
toolbox.register("attr_bool", random.randint, -1 * range_of_variable, range_of_variable)
toolbox.register("individual", tools.initRepeat, creator.Individual, toolbox.attr_bool, size_of_individual)
toolbox.register("population", tools.initRepeat, list, toolbox.individual)

def euclideanDistanceToWinRateCenter(winrate_vector):
	sum = 0
	
	for element in winrate_vector:
		sum += (element - 0.5) ** 2
	
	return sum ** 0.5

def sumBuffsNerfsAbs(meta_vector):
	sum = 0
	
	for element in meta_vector:
		sum += abs(element)
	
	return sum

def evalOneMin(individual, fitness_results):
	return fitness_results[individual],
	
toolbox.register("evaluate", evalOneMin)
toolbox.register("mate", tools.cxTwoPoint)
toolbox.register("mutate", tools.mutFlipBit, indpb=0.05)
toolbox.register("select", tools.selTournament, tournsize=3)

#fitness_results = []

def main():
	pop = toolbox.population(n=num_of_individuals)
	number_of_matchups = math.factorial(number_of_decks) / (2 * math.factorial(number_of_decks-2))
	
	# Variable keeping track of the number of generations
	g = 0
	saveToFile(directory + '/gen' + str(g) + '/input.txt', pop)
	
	while (number_of_workers > len([name for name in os.listdir(directory + '/gen' + str(g) + "/worker_data") if os.path.isfile(directory + '/gen' + str(g) + "/worker_data/" + name)])):
		time.sleep(10) #sleep for 10 seconds
	
	winrate_vectors = pwf.parseWorkerFiles(directory + '/gen' + str(g), number_of_matchups, number_of_workers)
	fitness_results = [euclideanDistanceToWinRateCenter(winrate_vectors[i]) for i in range(0, len(winrate_vectors))]
	
	# Evaluate the entire population
	fitnesses = list(map(functools.partial(toolbox.evaluate, fitness_results=fitness_results), range(0, len(pop))))
	for ind, fit in zip(pop, fitnesses):
		ind.fitness.values = fit
	
	# CXPB  is the probability with which two individuals
	#       are crossed
	#
	# MUTPB is the probability for mutating an individual
	CXPB, MUTPB = 0.35, 0.2
	
	# Extracting all the fitnesses of 
	fits = [ind.fitness.values[0] for ind in pop]
	
	files_to_delete = [name for name in os.listdir(directory + '/gen' + str(g) + "/worker_data") if os.path.isfile(directory + '/gen' + str(g) + "/worker_data/" + name)]
	for file in files_to_delete:
		os.remove(directory + '/gen' + str(g) + "/worker_data/" + file)
	saveToFile(directory + '/gen' + str(g) + '/output.txt', zip(pop, fits))

	# Begin the evolution
	while min(fits) > range_of_variable*(-1)*size_of_individual and g < 1000:
		# A new generation
		g = g + 1
		print("-- Generation %i --" % g)
	
		# Select the next generation individuals
		offspring = toolbox.select(pop, len(pop))
		# Clone the selected individuals
		offspring = list(map(toolbox.clone, offspring))

		# Apply crossover and mutation on the offspring
		for child1, child2 in zip(offspring[::2], offspring[1::2]):
			if random.random() < CXPB:
				toolbox.mate(child1, child2)
				del child1.fitness.values
				del child2.fitness.values

		for mutant in offspring:
			if random.random() < MUTPB:
				toolbox.mutate(mutant)
				del mutant.fitness.values
				
		# Evaluate the individuals with an invalid fitness
		invalid_ind = [ind for ind in offspring if not ind.fitness.valid]
		saveToFile(directory + '/gen' + str(g) + '/input.txt', invalid_ind)

		while (number_of_workers > len([name for name in os.listdir(directory + '/gen' + str(g) + "/worker_data") if os.path.isfile(directory + '/gen' + str(g) + "/worker_data/" + name)])):
			time.sleep(10) #sleep for 10 seconds

		winrate_vectors = pwf.parseWorkerFiles(directory + '/gen' + str(g), number_of_matchups, number_of_workers)
		fitness_results = [euclideanDistanceToWinRateCenter(winrate_vectors[i]) for i in range(0, len(winrate_vectors))]
	
		fitnesses = map(functools.partial(toolbox.evaluate, fitness_results=fitness_results), range(0, len(invalid_ind)))
		#fitnesses = map(toolbox.evaluate, invalid_ind)
		for ind, fit in zip(invalid_ind, fitnesses):
			ind.fitness.values = fit

		pop[:] = offspring
		
		# Gather all the fitnesses in one list and print the stats
		fits = [ind.fitness.values[0] for ind in pop]

		files_to_delete = [name for name in os.listdir(directory + '/gen' + str(g) + "/worker_data") if os.path.isfile(directory + '/gen' + str(g) + "/worker_data/" + name)]
		for file in files_to_delete:
			os.remove(directory + '/gen' + str(g) + "/worker_data/" + file)
		saveToFile(directory + '/gen' + str(g) + '/output.txt', zip(pop, fits))

		length = len(pop)
		mean = sum(fits) / length
		sum2 = sum(x*x for x in fits)
		std = abs(sum2 / length - mean**2)**0.5

		print("  Pop %s" % len(pop))
		print("  Min %s" % min(fits))
		print("  Max %s" % max(fits))
		print("  Avg %s" % mean)
		print("  Std %s" % std)
		
	print("-- End of (successful) evolution --")

	best_ind = tools.selBest(pop, 1)[0]
	print("Best individual is %s, %s" % (best_ind, best_ind.fitness.values))

main()