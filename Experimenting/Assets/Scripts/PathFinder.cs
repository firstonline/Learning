

using System.Collections.Generic;

public static class PathFinder
{
   public static  List<Walkable> FindPath(Walkable start, Walkable end)
   {
      List<Walkable> path = new List<Walkable>(); 
      List<Walkable> previousWalkables = new List<Walkable>();
      List<Walkable> nextWalkables = new List<Walkable>();
      bool pathFound = false;

      if (CalculatePathsFromCurrent(start, nextWalkables, previousWalkables, end))
      {
         pathFound = true;
      }
      else
      {
         while (nextWalkables.Count > 0)
         {
            var currentWalkable = nextWalkables[0];
            nextWalkables.RemoveAt(0);
            if (CalculatePathsFromCurrent(currentWalkable, nextWalkables, previousWalkables, end))
            {
               pathFound = true;
               break;
            }
         }
      }

      if (pathFound)
      {
         var currentWalkable = end;
         path.Add(end);
         while (currentWalkable.PreviousWalkable != start)
         {
            currentWalkable = currentWalkable.PreviousWalkable;
            path.Insert(0, currentWalkable);
         }
      }

      return path;
   }

   // returns true if current contains goes to end walkable
   private static bool CalculatePathsFromCurrent(Walkable current, List<Walkable> next, List<Walkable> previousWalkables, Walkable end)
   {
      bool containsEndWalkable = false;
      var possiblePaths = current.PossiblePaths;
      for (int i = 0; i < possiblePaths.Count; i++)
      {
         if (!previousWalkables.Contains(possiblePaths[i]))
         {
            next.Add(possiblePaths[i]);
            possiblePaths[i].PreviousWalkable = current;
            if (possiblePaths[i] == end)
            {
               containsEndWalkable = true;
               // no need to continue searching anymore
               break;
            }
         }
      }

      previousWalkables.Add(current);
      return containsEndWalkable;
   }
}
