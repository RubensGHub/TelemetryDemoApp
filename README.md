# TelemetryDemoApp
Ce rapport est constitué de deux parties :  
1. Définitions et explications de l'observabilité et de la télémétrie
2. Démonstration avec l'intéraction entre deux applications basiques 

## Partie 1 - Explications de la télémétrie dotnet

L'objectif de ce rapport est de fournir une vue d'ensemble des techniques et des outils de surveillance et d'analyse des systèmes distribués. Dans un contexte où les architectures de microservices et les environnements cloud natifs sont de plus en plus courants, il est crucial de comprendre comment surveiller efficacement les performances et les comportements de ces systèmes.  
Cela se rapporte au sujet de la télémétrie. Ce-dernier est étroitement lié à l'observabilité, qui consiste à analyser l'état et mesurer les performances de notre système/application, sans avoir à forcément comprendre comment fonctionne chaque élément de l'application.     
Nous explorerons les définitions, l'importance, les cas d'utilisation et les outils disponibles pour les trois données "pilliers" de la télémétrie :

* Logs
* Traces
* Metrics

### OpenTelemetry  

OpenTelemetry est un projet Open-source en plein développement supporté par la CNCFP. Il a donc de nombreux contributeurs qui améliorent régulièrement l'outil.  
Cet outil fourni tout un ensemble de bibliothèques et de technologies (APIs, SDKs, Agent) pour recevoir des logs, traces et métriques de différences sources et les traiter (si on utilise l'openTelemetry Collector) avant de les exporter à de nombreux outils de back-end.  
l'objectif d'OpenTelemetry est de créer un standard pour instrumenter nos applications, c'est à dire un outil qui s'adapte peu importe le langage et l'outil de back-end utilisé. Il peut ainsi capturer des données de télémétrie de différences sources et les exporter à différentes plateformes de stockage/d'analyse des données tout cela sans trop de modifications de configuration. Ainsi, les équipes peuvent utiliser l'outil de back-end qu'ils préfèrent.  

Etudions donc ces différentes données :

### Logs

#### Définition des logs

Les logs sont des enregistrements d'événements produits par les applications, les services et les systèmes d'exploitation. Ils jouent un rôle crucial dans le diagnostic des problèmes, la compréhension des comportements des systèmes et le suivi des activités.  
Les logs peuvent inclure des informations sur les erreurs, les transactions utilisateur, les accès aux ressources etc.  


#### Cas d'utilisation des logs

* **Diagnostic et Débogage** : Identifier et résoudre les erreurs et les anomalies dans les applications.
* **Sécurité** : Suivre les tentatives d'accès et détecter les comportements suspects.
* **Audit et Conformité** : Maintenir des enregistrements des activités pour répondre aux exigences réglementaires.

#### Les différents outils de logging

Après avoir collecté des logs avec fluendt, logstash, ou ici OpenTelemetry , nous pouvons exporter les données vers des services de back-end. Voici un tableau comparatif des différentes solutions envisageables et leurs avantages :

| Critères / Outils | Grafana Loki | Elasticsearch | Graylog |
|-------------------|------|---------------|---------|
| **Description**   | Outil de logging optimisé pour Grafana. | Moteur de recherche et d’analyse distribué. Fait partie de la suite ELK. | Outil de gestion de logs open-source. |
| **Intégration**   | Efficace avec Grafana et Prometheus. Conçu pour être utilisé dans des environnements Kubernetes. | Intégration avec Kibana pour la visualisation et Logstash pour l'ingestion. | Intégration avec divers systèmes comme Elasticsearch et MongoDB. |
| **Stockage**      | Architecture orientée événements pour stocker les logs, ce qui le rend très efficace pour les environnements à grande échelle où de nombreux logs sont générés. | Stockage distribué et évolutif avec une capacité de recherche en temps réel. | Stockage flexible utilisant Elasticsearch ou MongoDB, capable de gérer de grandes quantités de logs. |
| **Scalabilité**   | Haute scalabilité adaptée aux environnements Kubernetes. | Très scalable, adapté aux grandes infrastructures grâce à sa nature distribuée. | Scalabilité dépendante du backend de stockage (Elasticsearch ou MongoDB). |
| **Performance**   | Performant dans les environnements avec un fort volume de logs, grâce à son architecture optimisée. | Haute performance pour la recherche et l'analyse en temps réel de grandes quantités de données. | Performance fiable avec une capacité de gestion de logs en temps réel et des fonctionnalités avancées d'analyse. |
| **Facilité d'utilisation** | Installation et configuration simplifiées, particulièrement avec Grafana. | Peut être complexe à configurer en raison des nombreuses possibilités et des besoins en infrastructure. | Interface utilisateur intuitive, mais peut nécessiter une configuration initiale complexe. |
| **Communauté / Support** | Bonne communauté avec un support actif, particulièrement autour de l'écosystème Grafana. | Grande communauté avec beaucoup de ressources disponibles, support commercial disponible. | Communauté active avec de nombreux plugins et extensions disponibles. |

>[!TIP]
>
> Le choix de l'outil peut être adapté en fonction des besoins. OpenTelemetry fait en sorte que le passage d'un outil à un autre soit simple.
> D'autres choix (pas forcément exportable à partir d'OpenTelemetry, comme Graylog) incluent par exemple Splunk, 
> Dans notre projet d'exemple nous choisirons Loki. Ce-dernier est moins lourd qu'elasticsearch et plus efficace pour des cas d'utilisation.

### Traces

#### Définition des traces

Les traces distribuées sont des enregistrements des transactions qui se déroulent à travers les différents composants d'un système distribué. Elles permettent de suivre le parcours complet d'une requête, de son entrée dans le système à sa sortie, en passant par tous les services intermédiaires.
Ces informations peuvent contenir de la metadata comme le temps que prend chaque opération à se réaliser, ce qui peut être utile pour détecter des anomalies.

#### Cas d'utilisation des traces

* **Analyse de Performance** : Identifier les goulots d'étranglement et optimiser les performances des applications.
* **Diagnostic de Pannes** : Comprendre les causes des défaillances et des erreurs dans les transactions.
* **Visualisation des Flux** : Avoir une vue d'ensemble des interactions entre les services dans une architecture microservices.
  
#### Comparatif des outils pour les traces

De la même façon que les logs, il existe différents outils pour traiter et visualiser les traces reçues. Voici les principaux :

| Critères / Outils      | Jaeger  | Zipkin | Grafana Tempo |
|------------------------|---------|--------|---------------|
| **Description**        | Jaeger est une plateforme open source pour le traçage distribué développée par Uber et maintenant maintenue par la CNCF. | Zipkin est un outil de traçage distribué open source initialement développé par Twitter. | Grafana Tempo est une plateforme open source de traçage distribué développée par Grafana Labs, conçue pour être scalable et simple. |
| **Facilité d'installation** | Relativement simple à installer avec des options de déploiement via Docker, Kubernetes et d'autres orchestrateurs. | Simple à installer avec des options de déploiement via Docker et Kubernetes. | Simple à installer et à intégrer avec Grafana, avec des options de déploiement via Docker et Kubernetes. |
| **Visualisation**      | Offre une interface utilisateur riche avec des capacités de visualisation de traces détaillées et des outils d'analyse. | Dispose d'une interface utilisateur claire et efficace, mais légèrement moins riche en fonctionnalités que Jaeger. | Intégré nativement avec Grafana pour une visualisation puissante et une analyse des traces. |
| **Intégrations**       | Supporte de nombreuses intégrations avec des outils comme Prometheus, Grafana, et OpenTelemetry. | Bon support d'intégration avec divers outils, mais un peu moins étendu que Jaeger. | Intégration transparente avec Grafana et Prometheus, ainsi qu'avec d'autres outils d'observabilité. |
| **Scalabilité**        | Conçu pour être hautement scalable, capable de gérer de grandes quantités de données de traçage dans des environnements distribués complexes. | Scalabilité efficace, mais peut nécessiter plus de configuration pour des environnements très larges. | Hautement scalable, conçu pour gérer de grandes quantités de traces de manière efficace et avec une faible empreinte mémoire. |
| **Communauté / Support** | Grande communauté active, avec un soutien commercial disponible et une abondance de ressources et de documentations. | Bonne communauté et support, mais légèrement moins étendu que celui de Jaeger. | Bonne communauté, soutenue par Grafana Labs, avec une documentation complète et des options de support commercial. |


>[!TIP]
>
> Le choix de l'outil peut être adapté en fonction des besoins. OpenTelemetry fait en sorte que le passage d'un outil à un autre soit simple.
> D'autres choix incluent Honeycomb, Azure Monitor...
> Ici, il sera préférable d'utiliser Jaeger pour des environnements demandant plus de scalabilité.  
> Zipkin est plus efficace pour des demandes plus simples ou pour parfois mieux visualiser les traces en cascade.

### Metrics

#### Définition des metrics

Les métriques sont des mesures quantitatives sur les performances, les ressources et l'état des systèmes. Elles incluent des informations telles que l'utilisation du CPU, la latence des requêtes, le taux d'erreur, et plus encore. Les métriques sont essentielles pour le monitoring en temps réel et la gestion des performances des systèmes.

#### Cas d'utilisation des metrics

* **Monitoring en Temps Réel** : Surveiller l'état des systèmes et des applications pour détecter et réagir rapidement aux problèmes.
* **Optimisation des Performances** : Analyser les tendances et les anomalies pour améliorer les performances des systèmes.
* **Alerting** : Déclencher des alertes basées sur des seuils définis pour prévenir les incidents avant qu'ils ne deviennent critiques.

#### Comparatif des outils pour les metrics

Enfin, voici quelques outils de monitoring :

| Critères / Outils             | Prometheus | Graphite | InfluxDB |
|-------------------------------|------------|----------|----------|
| **Description**               | Système de surveillance et d'alerte open-source conçu pour les métriques et les séries temporelles. | Base de données de séries temporelles conçue pour le stockage et la visualisation des métriques. | Base de données de séries temporelles conçue pour les applications de surveillance, de journalisation et de gestion de flux de données en temps réel. |
| **Modèle de données**         | Modèle de données multidimensionnel avec identifiants par nom de métrique et paire clé-valeur. | Modèles de données hiérarchiques et basés sur des tags. | Modèle de données basé sur des séries temporelles avec support pour les tags et les champs. |
| **Langage de requête**        | Langage de requête PromQL pour générer des graphes, tables et alertes. | Langage de requête basé sur le pipeline de fonctions pour agréger et résumer les données. | Langage de requête Flux et SQL-like pour requêtes avancées et manipulation des données. |
| **Instrumentation**           | Collecte des métriques en "pull". | Envoi des métriques en "push". | Collecte des métriques en "push" et "pull". |
| **Scalabilité**               | Haute scalabilité horizontale avec Thanos et Cortex. | Scalabilité verticale limitée, solutions tierces nécessaires pour l’horizontale. | Haute scalabilité horizontale avec InfluxDB Enterprise et clustering. |
| **Performance**               | Très performant pour la collecte et la requête de métriques à haute fréquence. | Performant pour les métriques à basse et moyenne fréquence. | Très performant pour l'écriture et la lecture de grandes quantités de données en temps réel. |
| **Facilité d'utilisation**    | Relativement simple à installer et configurer. | Installation et configuration plus techniques. | Installation simple, interface utilisateur intuitive et riche. |
| **Communauté / Support**      | Grande communauté open-source, support commercial via des entreprises tierces. | Communauté active, mais moins de support commercial. | Grande communauté active, support commercial et documentation abondante. |



>[!TIP]
>
> Le choix de l'outil peut être adapté en fonction des besoins. OpenTelemetry fait en sorte que le passage d'un outil à un autre soit simple.
> D'autres choix incluent Datadog (couteux), New Relic, SignalFx, Dynatrace...
> Dans notre projet nous choisirons Prometheus. Les données de Prometheus, Tempo et Loki peuvent être visualisés facilement à l'aide de Grafana.

## Partie 2 - Utilisation de l'app de démo
