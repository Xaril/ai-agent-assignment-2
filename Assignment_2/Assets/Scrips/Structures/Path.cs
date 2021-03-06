﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    class Path<Node> : IEnumerable<Node>
    {
            
        public Node LastStep { get; private set; }
        public Path<Node> PreviousSteps { get; private set; }
        public int TotalCost { get; private set; }
        private Path(Node lastStep, Path<Node> previousSteps, int totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }
        public Path(Node start) : this(start, null, 0) { }
        public Path<Node> AddStep(Node step, int stepCost)
        {
            return new Path<Node>(step, this, TotalCost + stepCost);
        }
        public IEnumerator<Node> GetEnumerator()
        {
            for (Path<Node> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
