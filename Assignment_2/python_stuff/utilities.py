import numpy as np


def read_cost_matrix(filename):
    cost_matrix = np.genfromtxt(filename, delimiter=',', dtype='int')
    return cost_matrix
