# Multimodal - Equipe Welcome to PNS
## Informations générales
### Contexte du projet
“Vous avez été contacté par le département des nouvelles technologies de Polytech pour développer deux applications afin d’aider les nouveaux étudiants à s’intégrer dans la vie de l’école d’ingénieur”

## Appli AR
Scan de QR Codes positionnés à des endroits stratégiques de l’école pour fournir des infos sur la salle concernée (nombre de places disponibles, plan d’évacuation, objets présents).
### Lancement de l'application
1. Ouvrir Unity Hub
2. Créer un nouveau projet 3D Unity avec la version 2022.3.50f1 ou supérieure
3. Une fois le projet créé, importer le custom package ar_app.unitypackage (trouvable sur la branch main ou AR)
4. Dans la fenêtre d'import qui s'ouvre, cliquer sur "Import" en gardant tous les éléments sélectionnés
5. Une fois l'import terminé, ouvrir la scène principale qui se trouve dans Assets/Scenes/SampleScene
6. Dans Unity, aller dans File > Build Settings > Android > Switch Platform
7. Dans Player Settings, vérifier que :
   - L'orientation est en "Portrait"
   - Dans XR Plug-in Management (sous Project Settings), activer "AR Foundation"
   - Dans XR Plug-in Management > Android, activer "ARCore"
   - Les permissions de caméra sont activées dans Player Settings > Android > Other Settings
   - Dans Player Settings > Android :
     - Configuration > Target Architecture: cocher ARM64
     - Configuration > Scripting Backend: IL2CPP
     - Configuration > Api Compatibility Level*: .NET Standard 2.1
     - Other Settings > Rendering > Color Space: Linear
     - Other Settings > Graphics APIs: sélectionner uniquement OpenGLES3
     - Other Settings > Package Name: définir un nom de package unique
     - Other Settings > Minimum API Level: Android 7.0 'Nougat' (API level 24) ou supérieur
     - Other Settings > Target API Level: API level 33 (Android 13.0) recommandé
8. Connecter votre téléphone Android en mode développeur
9. Cliquer sur "Build And Run" pour lancer l'application sur le téléphone
10. Une fois lancé, manipuler en premier le filtre --> TIME --> 13h : si vous ne le faites pas, les informations ne seront pas chargées en touchant les blocs



**Note importante** : Assurez-vous que votre téléphone est compatible avec ARCore et que l'application ARCore est installée sur votre appareil. L'application a besoin d'un accès à la caméra pour fonctionner correctement.

### Limites
- L'application peut être utilisé en portrait ou paysage mais est moins ergonomique en portrait.
- En mode paysage, le drag and drop du panel de logs ne marche pas, le comportement est différent
- Il faut interagir en premier lieu avec les filtres pour faire fonctionner le remplissage d'informations dans les card
- Le panel des logs peut des fois apparaître complètement à l'opposé de l'utilisateur --> redémarrer l'application pour réinitialiser sa position et donc le voir en face de l'utilisateur
- Boutons de filtres et de logs constamment affichés --> pas ergonomique

### Sources
- https://www.youtube.com/watch?v=FWyTf3USDCQ&t=674s&ab_channel=samyam
- https://www.youtube.com/watch?v=GfS72wqKQ_g&ab_channel=immersiveinsiders


### Fonctionnalités implémentées
#### Fonctionnalités de base
- Visualisation augmentée de la map via des marqueurs AR: les Card instanciées sont attachées au cube pour rajouter plus de réalisme. Ce choix a été fait pour être confronter à la difficulté de la motion sickness et du tracking des objets lorsqu'ils sont instanciés dans le monde augmenté. Une 2e justification est de pouvoir accéder à plusieurs informations de plusieurs salles en même temps et pas être contraint a une seule card dans l'écran. Les utilisateurs sont libres d'afficher ou non les card qu'ils souhaitent.
- Affichage des informations des salles: Cliquer le cube d'une salle et vous verrez les informations
- Fournir des retours approprié pour les interactions
  
#### Fonctionnalités optionnelles
- Filtrage des données des étudiants présent dans la salle
- Possibilité de zoom ou dezoom sur les panels d’information lorsque vous avez cliqué sur un Cube dans l'application
- Génération de logs: via le bouton log en haut à gauche de l'écran, vous pourrez instancier en face de vous les logs (zoom dezoom, et drag and drop via la barre foncé horinzontal du haut)

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

### Sources
- Pour l'utilisation des joysticks, nous avons utilisé l'asset suivant : https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631
