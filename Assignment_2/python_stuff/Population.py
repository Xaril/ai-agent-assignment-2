from Chromosome import Chromosome
import numpy as np
from tqdm import tqdm
import copy


class Population:

    def __init__(self, population_size=50, adj=[], n_salesman=2):
        self.population = []
        self.population_size = population_size
        self.adj = adj
        self.n_cities = len(adj)
        self.n_salesman = n_salesman
        for i in range(population_size):
            self.population.append(Chromosome(number_of_cities=self.n_cities, number_of_traveling_salesman=self.n_salesman, adj=self.adj))

        self.history_tot_cost = []
        self.history_minmax = []
        self.history_score = []

    # Genetic Algorithm
    def run_genetic_algorithm(self, number_of_iterations=1000, mutation_probability=0.7, crossover_probability=0.7):

        # Run for a fixed number of iterations
        for it in tqdm(range(number_of_iterations)):

            # Tournament selection
            k = self.population_size
            j = (int)(self.population_size * 0.6)
            for _ in range(self.population_size - k):
                del self.population[-np.random.randint(0, len(self.population))]
            for _ in range(k - j):
                worst_chromosome_score = self.population[0].score
                worst_chromosome_index = 0
                for i in range(1, len(self.population)):
                    if self.population[i].score > worst_chromosome_score:
                        worst_chromosome_score = self.population[i].score
                        worst_chromosome_index = i
                del self.population[-worst_chromosome_index]

            for _ in range(self.population_size - len(self.population)):
                self.population.append(Chromosome(number_of_cities=self.n_cities, number_of_traveling_salesman=self.n_salesman, adj=self.adj))

            # Mutate globally
            for index in range(len(self.population)):
                if np.random.random(1)[0] < mutation_probability:
                    chromosome = copy.deepcopy(self.population[index])
                    chromosome.mutate_global()
                    if chromosome.score < self.population[index].score:
                        self.population[index] = chromosome

            # Mutate locally
            for index in range(len(self.population)):
                if np.random.random(1)[0] < mutation_probability:
                    chromosome = copy.deepcopy(self.population[index])
                    chromosome.mutate_local()
                    if chromosome.score < self.population[index].score:
                        self.population[index] = chromosome

            # Crossover
            for index1 in range(len(self.population)):
                if np.random.random(1)[0] < crossover_probability:
                    index2 = np.random.randint(0, len(self.population))
                    if index1 == index2:
                        index2 = np.random.randint(0, len(self.population))
                    child1 = copy.deepcopy(self.population[index1])
                    child2 = copy.deepcopy(self.population[index2])
                    child1.crossover(self.population[index2])
                    child2.crossover(self.population[index1])
                    if child1.score < self.population[index1].score:
                        self.population[index1] = child1
                    if child2.score < self.population[index2].score:
                        self.population[index2] = child2

            # Log the progress statistics in the history
            self.get_best_result(verbose=0)

    # Print the overall cost and the minmax cost of the best chromosome
    def get_best_result(self, verbose = 1):
        best_chromosome = self.population[0]
        for i in range(1, self.population_size):
            if self.population[i].score < best_chromosome.score:
                best_chromosome = self.population[i]
        if verbose == 0:
            self.history_minmax.append(best_chromosome.minmax)
            self.history_tot_cost.append(best_chromosome.cost)
            self.history_score.append(best_chromosome.score)
        if verbose == 1:
            print("Overall cost: ", best_chromosome.cost)
            print("Minmax cost: ", best_chromosome.minmax)
