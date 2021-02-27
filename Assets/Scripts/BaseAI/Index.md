# BaseAI Development Guide (v0.1.1)

This document is a short guide on the usage of the BaseAI component for the development of competitive racing AI within Race Baby Race.

## Methods

- **Void** SetDirection(**Vector2** newDirection)

  > Sets a new direction for the BaseAI to head in.
  >
  > newDirection.x Specifies the desired steering angle and may range from -1 to 1.
  >
  > newDirection.y Specifies the desired speed and may range from -1 to 1.

- **Vector3** GetNodes()

  > Gets an array of nodes in the BaseAI path in a sequential manner.
  >
  > (***Warning*** GetNodes() only returns sequential nodes provided they are also sequentially set up in the editor.)
  
- **Vector3** GetPlayerPositions()

  > Gets an array of player positions.
  
- **Item** GetCurrentItem()

  > Gets the current item the player is holding.

- **Void** UseItem()

  > Uses the current item
  >
  

## Variables

- **int** position

  > Contains the current position of the AI in the race. This int is assigned by the RaceManager.

## Todo

- Method for using items (finish)
- Visual for items

