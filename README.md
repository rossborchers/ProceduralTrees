# Procedural Trees
LSystem demonstration for Unity.
Based on the book [The Algorithmic Beauty of Plants](https://www.amazon.com/Algorithmic-Beauty-Plants-Virtual-Laboratory/dp/0387946764)

![Reproducing results in the book](https://media.giphy.com/media/St3rRONeAxaap0PEVK/giphy.gif)

Accurately reproduces patterns shown in the book.

![Looping L-system](https://media.giphy.com/media/cNNmnpP1awBVCYYRCV/giphy.gif)

The parameters have been modified a bit since the gif was taken but the underlying algorithm still works.
The example scene has a fully deterministic "random" loop. I used to render the gif above. There are some tricks already being used to improve performance such as pre-baking but unfortunately, it's still not necessarily realtime depending on your computer. 


![Seed](https://media.giphy.com/media/Q7Wf3iIPOP8y4xIRV5/giphy.gif)

A naive implementation using text. Quite slow but easy to understand. It allows you to map characters to arbitrary behavior by extending `LSystem.Module` It's pretty extensible but has some performance issues.

![Seed](https://media.giphy.com/media/f8PbJ8sqfWptR9b1Sq/giphy.gif)

There is also a procedural growing leaf script that generates a mesh on the fly and connects to the l-system. It's very expensive though.

**I would not use this in production without optimizing and fixing some bugs.**
