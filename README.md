# Explications de la télémétrie dotnet
L'objectif de ce rapport est de fournir une vue d'ensemble des techniques et des outils de surveillance et d'analyse des systèmes distribués. Dans un contexte où les architectures de microservices et les environnements cloud natifs sont de plus en plus courants, il est crucial de comprendre comment surveiller efficacement les performances et les comportements de ces systèmes.  
Ce rapport se concentre sur trois types de données principales : les logs, les traces distribuées et les métriques.  
Nous explorerons les définitions, l'importance, les cas d'utilisation et les outils disponibles pour chaque type de données.  
La télémétrie se rapporte à la surveillance et à l'analyse d'informations de systèmes informatiques visant à suivre les performances et à identifier les problèmes. Ce terme est directement lié au terme "d'observabilité".  
L'observabilité consiste à analyser l'état et mesurer les performances de notre système/application, sans avoir à forcément comprendre comment fonctionne chaque élément de l'application.  

L'utilisation d'OpenTelemetry se présentera également avec un exemple d'utilisation : https://github.com/RubensGHub/AppTestTelemetry.

## OpenTelemetry  
OpenTelemetry est un projet Open-source en plein développement supporté par la CNCFP. Il a donc de nombreux différents contributeurs qui améliore régulièrement l'outil.  
l'objectif d'OpenTelemetry est de créer un standard pour instrumenter nos applications, c'est à dire un outil qui s'adapte peu importe le langage et l'outil de back-end utilisé.  
OpenTelemetry peut récolter chacun des trois **pilliers** de la télémétrie :
* Logs
* Traces
* Metrics

## Logs 
### définition
Les logs sont des enregistrements d'événements produits par les applications, les services et les systèmes d'exploitation. Ils jouent un rôle crucial dans le diagnostic des problèmes, la compréhension des comportements des systèmes et le suivi des activités.  
Les logs peuvent inclure des informations sur les erreurs, les transactions utilisateur, les accès aux ressources, et bien plus encore

### Cas d'utilisation des logs
- **Diagnostic et Débogage** : Identifier et résoudre les erreurs et les anomalies dans les applications.
- **Sécurité** : Suivre les tentatives d'accès et détecter les comportements suspects.
- **Audit et Conformité** : Maintenir des enregistrements des activités pour répondre aux exigences réglementaires.

### Les différents outils de logging
Après avoir collecté des logs avec fluendt, logstash, ou dans ce projet OpenTelemetry Collector, nous pouvons exporter les données vers des services de back-end. Voici un tableau comparatif des différentes solutions envisageables et leurs avantages (ici nous utiliserons Loki) :

| Critères / Outils | Loki | Elasticsearch | Graylog |
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
> Dans notre projet d'exemple nous choisirons Loki. Ce-dernier est moins lourd qu'elasticsearch et plus efficace pour des cas d'utilisation.

## Traces
### Définition
Les traces distribuées sont des enregistrements des transactions qui se déroulent à travers les différents composants d'un système distribué. Elles permettent de suivre le parcours complet d'une requête, de son entrée dans le système à sa sortie, en passant par tous les services intermédiaires.

### Cas d'utilisation des traces
- **Analyse de Performance** : Identifier les goulots d'étranglement et optimiser les performances des applications.
- **Diagnostic de Pannes** : Comprendre les causes des défaillances et des erreurs dans les transactions.
- **Visualisation des Flux** : Avoir une vue d'ensemble des interactions entre les services dans une architecture microservices.
  
### Comparatif des outils pour les traces
De la même façon que les logs, il existe différents outils pour traiter et visualiser les traces reçues. Voici les principaux :

| Critères / Outils | Jaeger | Zipkin |
|-------------------|--------|--------|
| **Description**   | Jaeger est une plateforme open source pour le traçage distribué développée par Uber et maintenant maintenue par la CNCF. | Zipkin est un outil de traçage distribué open source initialement développé par Twitter. |
| **Facilité d'installation** | Relativement simple à installer avec des options de déploiement via Docker, Kubernetes et d'autres orchestrateurs.  | Simple à installer avec des options de déploiement via Docker et Kubernetes.  |
| **Visualisation** | Offre une interface utilisateur riche avec des capacités de visualisation de traces détaillées et des outils d'analyse. | Dispose d'une interface utilisateur claire et efficace, mais légèrement moins riche en fonctionnalités que Jaeger. |
| **Intégrations** | Supporte de nombreuses intégrations avec des outils comme Prometheus, Grafana, et OpenTelemetry. | Bon support d'intégration avec divers outils, mais un peu moins étendu que Jaeger. |
| **Scalabilité** | Conçu pour être hautement scalable, capable de gérer de grandes quantités de données de traçage dans des environnements distribués complexes. | Scalabilité efficace, mais peut nécessiter plus de configuration pour des environnements très larges.|
| **Communauté / Support** | Grande communauté active, avec un soutien commercial disponible et une abondance de ressources et de documentations.  | Bonne communauté et support, mais légèrement moins étendu que celui de Jaeger.  |

>[!TIP]
>
> Le choix de l'outil peut être adapté en fonction des besoins. OpenTelemetry fait en sorte que le passage d'un outil à un autre soit simple.  
> Ici, il sera préférable d'utiliser Jaeger pour des environnements demandant plus de scalabilité.  
> Zipkin est plus efficace pour des demandes plus simples ou pour parfois mieux visualiser les traces en cascade.


## Metrics
### Définition
Les métriques sont des mesures quantitatives sur les performances, les ressources et l'état des systèmes. Elles incluent des informations telles que l'utilisation du CPU, la latence des requêtes, le taux d'erreur, et plus encore. Les métriques sont essentielles pour le monitoring en temps réel et la gestion des performances des systèmes.

### Cas d'utilisation des metrics
- **Monitoring en Temps Réel** : Surveiller l'état des systèmes et des applications pour détecter et réagir rapidement aux problèmes.
- **Optimisation des Performances** : Analyser les tendances et les anomalies pour améliorer les performances des systèmes.
- **Alerting** : Déclencher des alertes basées sur des seuils définis pour prévenir les incidents avant qu'ils ne deviennent critiques.

### Comparatif des outils pour les metrics
Enfin, l'outil que nous utiliserons dans notre solution est Prometheus.   
Les données de Prometheus et Loki peuvent être visualisés facilement à l'aide de Grafana :

| Critères / Outils | Prometheus | Graphite | Datadog | InfluxDB |
|-------------------|------------|----------|---------|----------|
| **Description**   | Système de surveillance et d'alerte open-source conçu pour les métriques et les séries temporelles. | Base de données de séries temporelles conçue pour le stockage et la visualisation des métriques. | Plateforme de surveillance et d'analytique pour les infrastructures cloud. | Base de données de séries temporelles open-source conçue pour une haute performance. |
| **Intégration**   | Intégration native avec Kubernetes, Grafana et de nombreux exporters. | Intégration avec Grafana et des collecteurs comme collectd, StatsD. | Intégration avec de nombreux services cloud, infrastructure et applications. | Intégration avec Telegraf pour la collecte de métriques, et Grafana pour la visualisation. |
| **Stockage**      | Stockage local optimisé pour la haute performance et la rétention courte à moyenne. | Stockage orienté fichiers, optimisé pour la persistance des données à long terme. | Stockage dans le cloud, adapté pour la persistance à long terme et la scalabilité. | Stockage optimisé pour les séries temporelles avec compression efficace et haute performance d'écriture. |
| **Scalabilité**   | Haute scalabilité horizontale avec des solutions comme Thanos et Cortex. | Scalabilité verticale limitée, peut nécessiter des solutions tierces pour l’échelle horizontale. | Très scalable, conçu pour les environnements de grande échelle. | Scalabilité horizontale et verticale, avec capacité à gérer de grandes quantités de données. |
| **Performance**   | Très performant pour la collecte et la requête de métriques à haute fréquence. | Performant pour les métriques à basse et moyenne fréquence, moins pour les volumes élevés de données. | Haute performance avec des capacités d'analyse en temps réel et des alertes. | Haute performance pour l'ingestion et la requête de séries temporelles. |
| **Facilité d'utilisation** | Relativement simple à configurer avec une documentation complète et une grande communauté. | Installation et configuration plus techniques, nécessite des connaissances avancées. | Interface utilisateur intuitive, installation simple avec support commercial. | Interface utilisateur claire avec des outils comme Chronograf, mais nécessite une configuration initiale. |
| **Communauté / Support** | Grande communauté open-source, support commercial via des entreprises tierces. | Communauté active, mais plus restreinte, avec moins de support commercial. | Large communauté, support commercial direct et ressources abondantes. | Communauté active et grand nombre de ressources, support commercial via InfluxData. |


>[!TIP]
>
> Le choix de l'outil peut être adapté en fonction des besoins. OpenTelemetry fait en sorte que le passage d'un outil à un autre soit simple.
> Dans notre projet nous choisirons Prometheus.

