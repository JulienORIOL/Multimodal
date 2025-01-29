# Multimodal
## Appli VR
Création d’un espace virtuel où les étudiants peuvent habiller leur avatar et décorer leur espace virtuel avec des objets.
Je vais modifier la section du lancement de l'application VR pour expliquer comment utiliser le package Unity :

### Lancement de l'application

1. Ouvrir Unity Hub
2. Créer un nouveau projet 3D Unity avec la version 2022.3.16f1 ou supérieure
3. Une fois le projet créé, importer le custom package vr_app.unitypackage (trouvable sur la branch main ou VR)
4. Dans la fenêtre d'import qui s'ouvre, cliquer sur "Import" en gardant tous les éléments sélectionnés
5. Une fois l'import terminé, ouvrir la scène principale qui se trouve dans Assets/Scenes/MirrorScene
6. Dans Unity, aller dans File > Build Settings > Android > Switch Platform
7. Dans Player Settings, vérifier que :
   - L'orientation est en "Landscape Left"
   - Les configurations VR sont activées
8. Connecter votre téléphone Android en mode développeur
9. Cliquer sur "Build And Run" pour lancer l'application sur le téléphone

**Note importante** : Une fois l'application lancée, orienter la caméra vers où pointe la lampe torche. Au lancement, la lampe torche pointe vers un endroit fixe, c'est pour cela qu'il faut orienter la caméra vers où elle pointe, pour ne pas être perdu si vous ne voyez pas la lampe torche au départ.

### Fonctionnalités implémentées
#### Fonctionnalités de base
- Permettre à l'utilisateur de sélectionner et de placer des éléments dans une scène 3D à partir d'un menu, de les associer à une image ou à une vidéo de projet et de les modifier à l'aide d'une technique de sélection.
- Permettre à l'utilisateur de se déplacer dans la scène à l'aide d'une interaction multimodale.
- Fournir un retour d'information approprié pour les interactions
- L'application doit au minimum être mise en œuvre sur un téléphone ou une tablette avec 3DoF

#### Fonctionnalités optionnelles
- Changement de tenue de l’avatar
- Eclairage de la scène via une lampe torche personnalisable
- Miroir pour permettre à l’utilisateur de se voir dans la scène

### Limites
- Lancer uniquement sur téléphone (Android de préférence) avec format Landscape Left en paramètre de lancement.
- Une fois l'application lancé, commencer par orienter la caméra vers la où pointe la lampe torche. Au lancement, la lampe torche pointe vers un endroit fixe, c'est pour ca qu'il faut orienter la caméra vers la où elle pointe, pour ne pas être perdu si vous ne voyez pas la lampe torche au départ.