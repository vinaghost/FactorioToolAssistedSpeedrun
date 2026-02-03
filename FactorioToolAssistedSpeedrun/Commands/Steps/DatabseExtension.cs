using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public static class DatabseExtension
    {
        public static void AddSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            var minLocation = steps.Min(x => x.Location);
            context.Steps
                .Where(x => x.Location >= minLocation && x.Name == name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location + steps.Count));

            context.Steps.AddRange(steps);
            context.SaveChanges();
        }

        public static void DeleteSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            context.Steps
                .Where(x => steps.Select(s => s.Id).Contains(x.Id) && x.Name == name)
                .ExecuteDelete();

            var maxLocation = steps.Max(x => x.Location);
            context.Steps
                .Where(x => x.Location > maxLocation && x.Name == name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location - steps.Count));
        }

        public static void MoveSteps(this ProjectDbContext context, string name, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0) return;
            if (!context.Steps.Any(x => x.Name == name)) return;

            var count = context.Steps
                .Where(x => stepIds.Contains(x.Id))
                .Count();

            if (count == 0) return;
            if (moveOffset > 0)
            {
                // go down
                var lastStep = context.Steps
                    .Where(x => x.Name == name)
                    .OrderByDescending(x => x.Location)
                    .First();

                var lastLocation = context.Steps
                    .Where(x => stepIds.Contains(x.Id))
                    .OrderByDescending(x => x.Location)
                    .Select(x => x.Location)
                    .First();

                if (lastStep.Location == lastLocation)
                {
                    return;
                }

                if (lastLocation + moveOffset > lastStep.Location)
                {
                    moveOffset = lastStep.Location - lastLocation;
                }

                context.Steps
                   .Where(x => x.Location > lastLocation && x.Location <= lastLocation + moveOffset && x.Name == name)
                   .ExecuteUpdate(setters => setters
                       .SetProperty(b => b.Location, b => b.Location - count));
            }
            else
            {
                // go up
                var firstStep = context.Steps
                    .Where(x => x.Name == name)
                    .OrderBy(x => x.Location)
                    .First();

                var firstLocation = context.Steps
                    .Where(x => stepIds.Contains(x.Id))
                    .OrderBy(x => x.Location)
                    .Select(x => x.Location)
                    .First();

                if (firstStep.Location == firstLocation)
                {
                    return;
                }
                if (firstLocation + moveOffset < firstStep.Location)
                {
                    moveOffset = firstStep.Location - firstLocation;
                }
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + moveOffset && x.Name == name)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + count));
            }

            context.Steps
                .Where(x => stepIds.Contains(x.Id))
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + moveOffset));
        }

        public static void UpdatePosition(this ProjectDbContext context, string name, double oldX, double oldY, double newX, double newY)
        {
            context.Steps
                .Where(x => x.Name == name)
                .Where(x => Math.Abs(x.X - oldX) < 0.0001 && Math.Abs(x.Y - oldY) < 0.0001)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => newX)
                    .SetProperty(b => b.Y, b => newY));
        }
    }
}