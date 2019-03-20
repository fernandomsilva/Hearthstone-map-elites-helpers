import random, os, sys

from deap import base
from deap import creator
from deap import tools

range = 3
size_of_individual = 5
num_of_individuals = 300

def make_dir(dir):
	if not os.path.exists(dir):
		os.makedirs(dir)

directory = sys.argv[1]
number_of_workers = int(sys.argv[2])
make_dir(directory)

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
toolbox.register("attr_bool", random.randint, -1 * range, range)
toolbox.register("individual", tools.initRepeat, creator.Individual, toolbox.attr_bool, size_of_individual)
toolbox.register("population", tools.initRepeat, list, toolbox.individual)

def evalOneMax(individual):
	return sum(individual),
	
toolbox.register("evaluate", evalOneMax)
toolbox.register("mate", tools.cxTwoPoint)
toolbox.register("mutate", tools.mutFlipBit, indpb=0.05)
toolbox.register("select", tools.selTournament, tournsize=3)

def main():
	pop = toolbox.population(n=num_of_individuals)
	
	# Evaluate the entire population
	fitnesses = list(map(toolbox.evaluate, pop))
	for ind, fit in zip(pop, fitnesses):
		ind.fitness.values = fit
	
	# CXPB  is the probability with which two individuals
	#       are crossed
	#
	# MUTPB is the probability for mutating an individual
	CXPB, MUTPB = 0.35, 0.2
	
	# Extracting all the fitnesses of 
	fits = [ind.fitness.values[0] for ind in pop]
	
	# Variable keeping track of the number of generations
	g = 0
	saveToFile(directory + '/gen' + str(g) + '/input.txt', pop)
	saveToFile(directory + '/gen' + str(g) + '/output.txt', fits)
	# Begin the evolution
	while min(fits) > range*(-1)*size_of_individual and g < 1000:
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
		print(len(invalid_ind))
		fitnesses = map(toolbox.evaluate, invalid_ind)
		for ind, fit in zip(invalid_ind, fitnesses):
			ind.fitness.values = fit

		pop[:] = offspring
		saveToFile(directory + '/gen' + str(g) + '/input.txt', pop)
		
		# Gather all the fitnesses in one list and print the stats
		fits = [ind.fitness.values[0] for ind in pop]
		saveToFile(directory + '/gen' + str(g) + '/output.txt', fits)

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