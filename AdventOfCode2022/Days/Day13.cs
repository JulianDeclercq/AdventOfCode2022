﻿using System.Collections;

namespace AdventOfCode2022.Days;

public class Day13
{
    private class Element
    {
        public int Value = 0;
        public List<Element> Contents = new();
        public ListElementType Type = ListElementType.None;
    }

    private enum ListElementType
    {
        None = 0,
        Integer = 1,
        List = 2
    }
    
    public void Solve()
    {
        var pairs = File.ReadAllLines(@"..\..\..\input\day13.txt")
            .Where(l => !string.IsNullOrEmpty(l))
            .Chunk(2)
            .ToArray();

        var sum = 0;
        for (var i = 0; i < pairs.Length; ++i)
        {
            var pair = pairs[i];
            var left = ParseElement(pair.First());
            var right = ParseElement(pair.Last());
            var isOrdered = IsOrdered(left, right);
            
            if (isOrdered == null)
                throw new Exception("Indecisive comparison");
            
            if (isOrdered.Value)
                sum += i + 1;
        }
        Console.WriteLine($"Day 13 part 1: {sum}");
    }

    private static bool? IsOrdered(Element left, Element right)
    {
        if (left.Type is ListElementType.Integer && right.Type is ListElementType.Integer)
        {
            if (left.Value < right.Value)
                return true;
            
            if (left.Value > right.Value)
                return false;

            return null;
        }

        if (left.Type is ListElementType.List && right.Type is ListElementType.List)
        {
            var loopLength = Math.Max(left.Contents.Count, right.Contents.Count);
            for (var i = 0; i < loopLength; ++i)
            {
                // if undecided and left runs out of items first, the inputs are in the right order
                if (i >= left.Contents.Count)
                    return true;
                
                // if undecided and right runs out of items first, the inputs are not in the right order
                if (i >= right.Contents.Count)
                    return false;
                
                var ordered = IsOrdered(left.Contents[i], right.Contents[i]);
                if (ordered != null)
                    return ordered;
            }
            
            // If the lists are the same length and no comparison makes a decision about the order, continue checking the next part of the input.
            if (left.Contents.Count != right.Contents.Count)
                throw new Exception("THIS SHOULD REALLY NEVER HAPPEN");

            return null;
        }
        
        /* If exactly one value is an integer, convert the integer to a list which contains that integer as its only value,
         * then retry the comparison. For example, if comparing [0,0,0] and 2, convert the right value to [2] (a list containing 2);
         * the result is then found by instead comparing [0,0,0] and [2]. */

        if (left.Type is ListElementType.Integer)
            return IsOrdered(ConvertIntegerToList(left), right);

        if (right.Type is ListElementType.Integer)
            return IsOrdered(left, ConvertIntegerToList(right));

        return false;
    }

    private static Element ConvertIntegerToList(Element element)
    {
        if (element.Type is not ListElementType.Integer)
            throw new Exception("Can't convert non-integer element.");
        
        return new Element
        {
            Type = ListElementType.List,
            Contents = new List<Element> { element }
        };
    }

    // Parses one level
    private static Element ParseElement(string element)
    {
        var stack = new Stack<int>();
        var elements = new List<Element>();
        var depth = 0;
        var currentInt = "";
        for (var i = 0; i < element.Length; ++i)
        {
            var debug = element[i..];
            var c = element[i];
            switch (c)
            {
                case '[':
                    stack.Push(i);
                    depth++;
                    break;
                case ']': 
                    var itemStart = stack.Pop();
                    depth--;
                    if (depth == 1) // TODO: 0?
                        elements.Add(ParseElement(element[itemStart..(i+1)]));
                    break;
                case ',':
                    if (string.IsNullOrEmpty(currentInt))
                        break;

                    if (depth == 1) // TODO: 0?
                    {
                        elements.Add(new Element
                        {
                            Type = ListElementType.Integer,
                            Value = int.Parse(currentInt) 
                        });
                    }
                    currentInt = "";
                    break;
                default:
                    if (depth == 1 && char.IsDigit(c))
                        currentInt += c;
                    break;
            }
        }

        // Add the last element, since there's no trailing comma for the last one
        if (currentInt.Any())
        {
            elements.Add(new Element
            {
                Type = ListElementType.Integer,
                Value = int.Parse(currentInt) 
            });
        }

        return new Element
        {
            Type = ListElementType.List,
            Contents = elements
        };
    }
}