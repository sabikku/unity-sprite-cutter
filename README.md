# unity-sprite-cutter

Simple solution for cutting sprites in your game.

What it does, exactly, with given GameObject:
 - converts SpriteRenderer to identical MeshFilter/MeshRenderer components
 - cuts through the mesh - the Texture itself isn't cut, all magic comes from setting "cutted" UVs
 - converts BoxCollider2D and CircleCollider2D into PolygonCollider2D
 - cuts through the collider paths
 - returns cutting result as two new GameObjects (so you can still do whatever you want with the original one) or as one ("cutting off" new GO from existing one)

# Limitations

 - EdgeCollider2D isn't supported.
 - Non-convex Collider2D shapes aren't supported.
