# OrToolsPickListSolver

Made with [OR-Tools](https://developers.google.com/optimization)

## Explanation of model

```
I = pick list lines
J = container lines
K = containers
M = items

P_i = quantity requested on pick list line i
Q_j = quantity available on container line j
P_m = Σ Q_i where i has item m
Q_m = Σ Q_j where j has item m
```

### Algorithm

CBC Mixed Integer Programming

### Variables

One integer variable for each possible pick list line / container line match
```
X_ij = quantity from container line j used to fulfill pick list line i
```

Boolean variables for each container indicating whether any line from the container is used
```
Y_k = Σ X_ij where j is a line on container k
Z_k = 1 if Y_k > 0 else 0
```

### Constraints

No more quantity is assigned to a pick list than the quantity requested
```
Σ X_ij <= Q_i for all i  
```

No more quantity is assigned from a container line than the quantity available
```
Σ X_ij <= Q_j for all j
```

Ensure that for a given item, either the total requested quantity is fulfilled, or there is none left
```
X_m = Σ X_ij where j has item m
X_m >= min { Q_m, P_m }
```

Constrain "usage" variables using the following method:

<https://cs.stackexchange.com/questions/104990/boolean-variable-that-captures-whether-an-inequality-holds>

### Objective

Minimize sum of boolean "usage" variables
```
Σ a_k*Z_k where a_k = 1 if container is "putaway", 3 if container is "replenishment"
```