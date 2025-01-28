# Multimodal - Equipe Welcome to PNS
## Informations générales
### Contexte du projet
“Vous avez été contacté par le département des nouvelles technologies de Polytech pour développer deux applications afin d’aider les nouveaux étudiants à s’intégrer dans la vie de l’école d’ingénieur”

## Appli AR
Scan de QR Codes positionnés à des endroits stratégiques de l’école pour fournir des infos sur la salle concernée (nombre de places disponibles, plan d’évacuation, objets présents).
### Lancement de l'application
1. Se placer dans la branche AR : `git checkout AR`

### Fonctionnalités implémentées
#### Fonctionnalités de base
- Visualisation augmentée de la map via des marqueurs AR
- Affichage des informations des salles
- Fournir des retours approprié pour les interactions
  
#### Fonctionnalités optionnelles
- Filtrage des données des étudiants présent dans la salle
- Possibilité de zoom ou dezoom sur les panels d’information
- Génération de logs

## Appli VR
Création d’un espace virtuel où les étudiants peuvent habiller leur avatar et décorer leur espace virtuel avec des objets.
### Lancement de l'application
1. Se placer dans la branche VR : `git checkout VR`
2. Dans le projet sur Unity pour le lancer l'application sur le téléphone : File > Build Settings > Android > Build And Run

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
