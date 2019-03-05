from utilities import read_cost_matrix
from Population import Population
import matplotlib.pyplot as plt
import numpy as np

if __name__ == '__main__':
    filename = './cost_matrix.txt'
    cost_matrix = read_cost_matrix(filename)

    all_cities = len(cost_matrix) - 1
    counted_cities = 0
    save_solution = False

    while counted_cities != all_cities:
        counted_cities = 0

        population = Population(population_size=30, adj=cost_matrix, n_salesman=3)
        population.run_genetic_algorithm(number_of_iterations=2000,
                                         mutation_probability=0.3,
                                         crossover_probability=0.5)
        population.get_best_result()

        # # print(cost_matrix)
        # sys.stdout.write('Lovely script!\n')
        # print('Cool')
        # np.save('./something_else.txt', cost_matrix)

        minmax = 0

        # Print best solution
        # Iterate through population and get the best solution
        best_chromosome = population.population[0]
        for i in range(1, population.population_size):
            if population.population[i].score < best_chromosome.score:
                best_chromosome = population.population[i]

        # Print best solution
        for i in range(best_chromosome.m):
            print(i, ":  ", best_chromosome.solution[i][0], end="", sep="")
            tot_cost = 0
            counted_cities -= 1
            for j in range(1, len(best_chromosome.solution[i])):
                print("-", best_chromosome.solution[i][j], end="", sep="")
                tot_cost += cost_matrix[best_chromosome.solution[i][j - 1]][best_chromosome.solution[i][j]]
                counted_cities += 1
            print(" --- #", len(best_chromosome.solution[i]), end="")
            print(" - cost: ", tot_cost, "\n")
            if tot_cost > minmax:
                minmax = tot_cost

        # Saving Solution
        if save_solution:
            with open('result_P2.txt', 'wb') as f:
                for row in best_chromosome.solution:
                    np.savetxt(f, [row], fmt='%i', delimiter=',')

        # Print cost
        print("Cost: ", best_chromosome.cost)
        print("Minmax: ", minmax)

        print(f"Tot cities: {all_cities}, visited cities: {counted_cities}")

        # Plotting training statistics
        fig, ax = plt.subplots(nrows=1, ncols=2, figsize=(10,4))
        ax[0].plot(population.history_tot_cost)
        ax[0].set_xlabel('Iteration')
        ax[0].set_ylabel('Total cost')
        ax[1].plot(population.history_minmax)
        ax[1].set_xlabel('Iteration')
        ax[1].set_ylabel('Minmax')
        fig.show()

