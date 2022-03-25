# SameHashProblem

This is a demo library, that measures the time to find three strings that return the same value from the `GetHashCode` function (i.e. it searches for hash collisions). The time is measured for a sequential and a a parallel executions.

To start the collision search run

```bash
dotnet test
```

The results of the run are written to `stdout`.