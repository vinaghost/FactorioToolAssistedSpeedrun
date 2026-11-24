namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public static class StepOrdering
    {
        // Returns newOrder where newOrder[newIndex] = oldIndex.
        // selectedIndices must be sorted ascending. Each selected step moves by moveOffset,
        // clamped to the list bounds, and selected steps keep their relative order.
        public static int[] Reorder(int count, IReadOnlyList<int> selectedIndices, int moveOffset)
        {
            var targets = new int[selectedIndices.Count];
            if (moveOffset > 0)
            {
                var upperBound = count;
                for (int i = selectedIndices.Count - 1; i >= 0; i--)
                {
                    targets[i] = Math.Min(selectedIndices[i] + moveOffset, upperBound - 1);
                    upperBound = targets[i];
                }
            }
            else
            {
                var lowerBound = -1;
                for (int i = 0; i < selectedIndices.Count; i++)
                {
                    targets[i] = Math.Max(selectedIndices[i] + moveOffset, lowerBound + 1);
                    lowerBound = targets[i];
                }
            }

            var newOrder = new int[count];
            Array.Fill(newOrder, -1);
            for (int i = 0; i < targets.Length; i++)
            {
                newOrder[targets[i]] = selectedIndices[i];
            }

            var selected = new HashSet<int>(selectedIndices);
            var next = 0;
            for (int position = 0; position < count; position++)
            {
                if (newOrder[position] != -1) continue;
                while (selected.Contains(next)) next++;
                newOrder[position] = next++;
            }
            return newOrder;
        }
    }
}
