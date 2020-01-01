# Procedural Trees
LSystem demonstration for Unity.
Based on the book [The Algorithmic Beauty of Plants](https://www.amazon.com/Algorithmic-Beauty-Plants-Virtual-Laboratory/dp/0387946764)

![Reproducing results in book](https://media.giphy.com/media/St3rRONeAxaap0PEVK/giphy.gif)

Accuratley reproduces patterns shown in book.

![Looping L-system](https://media.giphy.com/media/cNNmnpP1awBVCYYRCV/giphy.gif)

The parameters have been modified a bit since the gif was taken but the underlying algorithm still works.
The example scene has a fully deterministic "random" loop. I used to render the gif above. There are a number of tricks already being used to improve performance such as pre-baking, but unfortunatley its still not neccisarily realtime depending on your computer. 


![Seed](https://media.giphy.com/media/Q7Wf3iIPOP8y4xIRV5/giphy.gif)

A naive implementation using text. Quite slow but easy to understand. Allows you to map characts to arbritrary behaviour by extending `LSystem.Module` Its pretty extensible but has some performance issues.

![Seed](https://media.giphy.com/media/f8PbJ8sqfWptR9b1Sq/giphy.gif)

There is also a procedural leaf script that generates a mesh on the fly. Its too expensive to be used on trees though.

**I would not use this in production without optomizing and fixing some bugs.**
