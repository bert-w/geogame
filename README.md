# geogame
In our game player 1 plays against player 2. One of the players draws a polygon, while the other player needs to place the least number of lights to light it up.
We will use a radial sweep from each light to determine the visibility polygon. The Union of all visibility polygons decides whether the entire polygon is illuminated.


The source files are in this folder: https://github.com/bert-w/geogame/tree/master/Assets/Scripts
Here we included a file with basic classes, like Vertex, Edge, Triangle etc.
We also have a file which contains the class for the status and the queue for the radial sweep.
Lastly we have the GameController.cs file which contains classes for performing the radial sweep and calculating the total visibility.
