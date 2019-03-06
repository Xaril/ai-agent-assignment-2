from utilities import read_cost_matrix
from Population import Population
import matplotlib.pyplot as plt
import numpy as np
import argparse

parser = argparse.ArgumentParser(description='Runs a Genetic Algorithm to solve the minmax mTSP')
parser.add_argument('--problem', help='Filename with the cost matrix')
parser.add_argument('--solution', help='Filename of the computed solution')
parser.add_argument('-m', type=int, help='Number of salesman')
parser.add_argument('--population', type=int, help='Size of the population')
parser.add_argument('--iterations', type=int, help='Number of iterations')
parser.add_argument('--crossover', type=float, help='Crossover probability')
parser.add_argument('--mutation', type=float, help='Mutation probability')

args = parser.parse_args()

filename_problem = args.problem
filename_solution = args.solution
n_salesman = args.m
population_size = args.population
number_of_iterations = args.iterations
mutation_probability = args.mutation
crossover_probability = args.crossover

save_solution = True

if __name__ == '__main__':
    cost_matrix = read_cost_matrix(filename_problem)

    all_cities = len(cost_matrix) - 1 - n_salesman
    counted_cities = 0

    while counted_cities != all_cities:
        counted_cities = 0

        population = Population(population_size=population_size, adj=cost_matrix, n_salesman=n_salesman)
        population.run_genetic_algorithm(number_of_iterations=number_of_iterations,
                                         mutation_probability=mutation_probability,
                                         crossover_probability=crossover_probability)
        population.get_best_result()

        # # print(cost_matrix)
        # sys.stdout.write('Lovely script!\n')
        # print('Cool')
        # np.save('./something_else.txt', cost_matrix)

        minmax = 0

        # Iterate through population and get the best solution
        best_chromosome = population.best_absolute_chromosome # population.population[0]
        # for i in range(1, population.population_size):
        #     if population.population[i].score < best_chromosome.score:
        #         best_chromosome = population.population[i]

        # Print best solution
        for i in range(best_chromosome.m):
            print(i, ":  ", best_chromosome.solution[i][0], end="", sep="")
            tot_cost = 0
            counted_cities -= 1
            for j in range(1, len(best_chromosome.solution[i]) - 1):
                print("-", best_chromosome.solution[i][j], end="", sep="")
                tot_cost += cost_matrix[best_chromosome.solution[i][j - 1]][best_chromosome.solution[i][j]]
                counted_cities += 1
            print(" --- #", len(best_chromosome.solution[i]), end="")
            print(" - cost: ", tot_cost, "\n")
            if tot_cost > minmax:
                minmax = tot_cost

        # Saving Solution
        if save_solution:
            with open(args.solution, 'wb') as f:
                for row in best_chromosome.solution:
                    np.savetxt(f, [row], fmt='%i', delimiter=',')

        # Print cost
        print("Cost: ", best_chromosome.cost)
        print("Minmax: ", minmax)

        print(f"Tot cities: {all_cities}, visited cities: {counted_cities}")

        # Plotting training statistics
        fig, ax = plt.subplots(nrows=1, ncols=3, figsize=(4 * 3, 4))
        ax[0].plot(population.history_score)
        ax[0].set_xlabel('Iteration')
        ax[0].set_ylabel('Score')
        ax[1].plot(population.history_tot_cost)
        ax[1].set_xlabel('Iteration')
        ax[1].set_ylabel('Total cost')
        ax[2].plot(population.history_minmax)
        ax[2].set_xlabel('Iteration')
        ax[2].set_ylabel('Minmax')
        fig.show()
