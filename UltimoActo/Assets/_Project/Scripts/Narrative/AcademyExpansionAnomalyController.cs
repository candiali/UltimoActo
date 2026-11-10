using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum ExpansionNodeType
    {
        BasementAcousticPipesAnomaly = 0,
        BasementSecretMaintenanceLog = 1,
        RooftopSecretUnmappedWindow = 2,
        InfirmaryScreenShadowAnomaly = 3,
        InfirmarySecretTornMedicalRegister = 4,
        MemorialRemovedPlaqueSecret = 5,
        DormitoryMusicBoxAndTrunkAnomaly = 6,
        DormitoryCollectiveDiarySecret = 7,
        WorkshopTwinBlackSwanOrderSecret = 8,
        PatronageDinnerSecretMinutes = 9,
        IsadoraSpyLatticeSecret = 10,
        MezzanineImperialBalconyOverview = 11,
        TheoryGlassSlideProjectorSecret = 12,
        MaestroKrolAlteredScoreSecret = 13,
        ClockTowerJammedGearSecret = 14,
        BoilerIncinerationFurnaceSecret = 15,
        SubterraneanLaundryDumbwaiterSecret = 16,
        UnderstageTrapdoorMechanismSecret = 17,
        AncientCryptFoundersPassageSecret = 18,
        CarriageHouseConfiscatedTrunkSecret = 19,
        GatehouseCutSwitchboardSecret = 20,
        FrozenSwanPondGazeboSecret = 21
    }

    /// <summary>
    /// Controlador narrativo y de anomalías psicológicas para las secciones de expansión (Zonas 41 a 48):
    /// - Zona 41: Sótano de Mantenimiento (sonidos de la academia a través de las paredes/tuberías + registro secreto de uso del teatro tras la clausura).
    /// - Zona 42: Terraza y Azotea (orientación exterior + secreto de la ventana iluminada que no aparece en los planos de Administración).
    /// - Zona 43: Enfermería Antigua (sombra tras el biombo médico + registro clínico con anotación de la noche clave de Elena y página siguiente arrancada).
    /// - Zona 44: Sala Conmemorativa (ambiente solemne con flores secas + marca en la pared de la placa retirada).
    /// - Zona 45: Dormitorio Común de Internas (caja de música autónoma + secreto del diario colectivo en el baúl oculto).
    /// - Zona 46: Taller de Zapatillas y Tintorería Escénica (encargo secreto de dos trajes idénticos del Cisne Negro y hormas gemelas antes del estreno).
    /// - Zona 47: Comedor Antiguo y Salón del Patronato (acta reservada de la cena del 14 de octubre y bandejas nocturnas tras la clausura).
    /// - Zona 48: Estancia Privada de Isadora Vale (celosía de vigilancia oculta tras el muro y bitácora de espionaje nocturno).
    /// </summary>
    public class AcademyExpansionAnomalyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private ExpansionNodeType nodeType;
        [SerializeField] private Transform primaryTarget;
        [SerializeField] private Transform secondaryTarget;
        [SerializeField] private GameObject hiddenObjectTarget;
        [SerializeField] private Light targetLight;

        private Transform playerTransform;
        private AudioSource localAudio;
        private bool hasInteracted;
        private float ambientTimer;
        private Vector3 initialPosA;
        private Quaternion initialRotA;

        public string Id => $"academy_expansion_{nodeType}";

        public string Prompt
        {
            get
            {
                switch (nodeType)
                {
                    case ExpansionNodeType.BasementAcousticPipesAnomaly:
                        return "[E] Escuchar resonancia de la academia en las tuberías";
                    case ExpansionNodeType.BasementSecretMaintenanceLog:
                        return "[E] Examinar registro secreto de mantenimiento del teatro";
                    case ExpansionNodeType.RooftopSecretUnmappedWindow:
                        return "[E] Observar el tejado del teatro y comparar con los planos";
                    case ExpansionNodeType.InfirmaryScreenShadowAnomaly:
                        return hasInteracted
                            ? "[E] Examinar camilla vacía tras el biombo"
                            : "[E] Apartar biombo de observación médica";
                    case ExpansionNodeType.InfirmarySecretTornMedicalRegister:
                        return "[E] Examinar registro de enfermería (página arrancada)";
                    case ExpansionNodeType.MemorialRemovedPlaqueSecret:
                        return "[E] Inspeccionar huella de la placa retirada en la pared";
                    case ExpansionNodeType.DormitoryMusicBoxAndTrunkAnomaly:
                        return "[E] Examinar caja de música de bailarina sobre la cama";
                    case ExpansionNodeType.DormitoryCollectiveDiarySecret:
                        return "[E] Abrir baúl oculto de las alumnas internas";
                    case ExpansionNodeType.WorkshopTwinBlackSwanOrderSecret:
                        return "[E] Examinar encargo secreto de los dos trajes del Cisne Negro";
                    case ExpansionNodeType.PatronageDinnerSecretMinutes:
                        return "[E] Examinar acta secreta de la cena del Patronato";
                    case ExpansionNodeType.IsadoraSpyLatticeSecret:
                        return "[E] Mirar por la celosía de vigilancia y leer bitácora de Isadora";
                    case ExpansionNodeType.MezzanineImperialBalconyOverview:
                        return "[E] Inspeccionar protocolo de recepción del Balcón Imperial (Planta Alta)";
                    case ExpansionNodeType.TheoryGlassSlideProjectorSecret:
                        return "[E] Encender linterna mágica y examinar placas de cristal anatómicas";
                    case ExpansionNodeType.MaestroKrolAlteredScoreSecret:
                        return "[E] Examinar manuscrito tachado del Maestro Viktor Krol (Acto III)";
                    case ExpansionNodeType.ClockTowerJammedGearSecret:
                        return "[E] Inspeccionar engranaje central bloqueado a las 20:15 en la Torre del Reloj";
                    case ExpansionNodeType.BoilerIncinerationFurnaceSecret:
                        return "[E] Abrir compuerta de cenizas de la Caldera N.º 3 y leer registro";
                    case ExpansionNodeType.SubterraneanLaundryDumbwaiterSecret:
                        return "[E] Inspeccionar terminal subterránea del montacargas y sábanas con cloral";
                    case ExpansionNodeType.UnderstageTrapdoorMechanismSecret:
                        return "[E] Examinar cerrojo saboteado bajo el escotillón del Escenario Principal";
                    case ExpansionNodeType.AncientCryptFoundersPassageSecret:
                        return "[E] Examinar lápida fundacional y plano del conducto del siglo XVIII";
                    case ExpansionNodeType.CarriageHouseConfiscatedTrunkSecret:
                        return "[E] Inspeccionar baúl de gira confiscado a Elena Vesper en la Cochera";
                    case ExpansionNodeType.GatehouseCutSwitchboardSecret:
                        return "[E] Examinar centralita telefónica cortada en el Pabellón del Guarda";
                    case ExpansionNodeType.FrozenSwanPondGazeboSecret:
                        return "[E] Examinar violín sumergido y corona en el Estanque de los Cisnes";
                    default:
                        return "[E] Examinar";
                }
            }
        }

        public bool CanInteract(GameObject interactor) => true;

        public void Configure(
            ExpansionNodeType type,
            Transform primary = null,
            Transform secondary = null,
            GameObject hiddenObj = null,
            Light lightRef = null)
        {
            nodeType = type;
            primaryTarget = primary;
            secondaryTarget = secondary;
            hiddenObjectTarget = hiddenObj;
            targetLight = lightRef;
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            if (primaryTarget != null)
            {
                initialPosA = primaryTarget.localPosition;
                initialRotA = primaryTarget.localRotation;
            }

            SetupAudioIfNeeded();
        }

        private void SetupAudioIfNeeded()
        {
            localAudio = gameObject.AddComponent<AudioSource>();
            localAudio.spatialBlend = 1f;
            localAudio.rolloffMode = AudioRolloffMode.Linear;

            if (nodeType == ExpansionNodeType.BasementAcousticPipesAnomaly)
            {
                localAudio.minDistance = 2.0f;
                localAudio.maxDistance = 18.0f;
                localAudio.loop = true;
                localAudio.volume = 0.24f;
                localAudio.clip = CreateBasementWallResonanceClip();
                localAudio.Play();
            }
            else if (nodeType == ExpansionNodeType.InfirmaryScreenShadowAnomaly)
            {
                localAudio.minDistance = 1.5f;
                localAudio.maxDistance = 9.0f;
                localAudio.loop = true;
                localAudio.volume = 0.16f;
                localAudio.clip = CreateFaintBreathingAndRustleClip();
                localAudio.Play();
            }
            else if (nodeType == ExpansionNodeType.DormitoryMusicBoxAndTrunkAnomaly)
            {
                localAudio.minDistance = 1.5f;
                localAudio.maxDistance = 12.0f;
                localAudio.loop = true;
                localAudio.volume = 0.20f;
                localAudio.clip = CreateDetunedMusicBoxClip();
                localAudio.Play();
            }
            else
            {
                localAudio.minDistance = 1.5f;
                localAudio.maxDistance = 10.0f;
                localAudio.loop = false;
                localAudio.playOnAwake = false;
                localAudio.volume = 0.35f;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
                if (playerTransform == null) return;
            }

            ambientTimer += Time.deltaTime;

            if (nodeType == ExpansionNodeType.BasementAcousticPipesAnomaly)
            {
                if (primaryTarget != null)
                {
                    float vib = Mathf.Sin(ambientTimer * 14f) * 0.004f * Mathf.Max(0f, Mathf.Sin(ambientTimer * 0.8f));
                    primaryTarget.localPosition = initialPosA + new Vector3(vib, 0f, -vib);
                }
            }
            else if (nodeType == ExpansionNodeType.InfirmaryScreenShadowAnomaly && !hasInteracted)
            {
                float dist = Vector3.Distance(playerTransform.position, transform.position);
                if (dist < 2.2f && hiddenObjectTarget != null && hiddenObjectTarget.activeSelf)
                {
                    hiddenObjectTarget.SetActive(false);
                    if (localAudio != null && localAudio.isPlaying)
                    {
                        localAudio.Stop();
                    }
                }
            }
            else if (nodeType == ExpansionNodeType.RooftopSecretUnmappedWindow)
            {
                if (targetLight != null)
                {
                    float baseIntensity = hasInteracted ? 4.2f : 2.1f;
                    targetLight.intensity = baseIntensity + Mathf.Sin(ambientTimer * 2.3f) * 0.35f;
                }
            }
            else if (nodeType == ExpansionNodeType.DormitoryMusicBoxAndTrunkAnomaly && !hasInteracted)
            {
                if (primaryTarget != null)
                {
                    primaryTarget.Rotate(Vector3.up, 32f * Time.deltaTime, Space.Self);
                }
                float dist = Vector3.Distance(playerTransform.position, transform.position);
                if (dist < 2.0f && localAudio != null && localAudio.isPlaying)
                {
                    localAudio.Stop();
                }
            }
        }

        public void Interact(GameObject interactor)
        {
            switch (nodeType)
            {
                case ExpansionNodeType.BasementAcousticPipesAnomaly:
                    HandleBasementAcousticPipes();
                    break;
                case ExpansionNodeType.BasementSecretMaintenanceLog:
                    HandleBasementSecretLog();
                    break;
                case ExpansionNodeType.RooftopSecretUnmappedWindow:
                    HandleRooftopSecretWindow();
                    break;
                case ExpansionNodeType.InfirmaryScreenShadowAnomaly:
                    HandleInfirmaryScreenAnomaly();
                    break;
                case ExpansionNodeType.InfirmarySecretTornMedicalRegister:
                    HandleInfirmaryTornRegister();
                    break;
                case ExpansionNodeType.MemorialRemovedPlaqueSecret:
                    HandleMemorialRemovedPlaque();
                    break;
                case ExpansionNodeType.DormitoryMusicBoxAndTrunkAnomaly:
                    HandleDormitoryMusicBox();
                    break;
                case ExpansionNodeType.DormitoryCollectiveDiarySecret:
                    HandleDormitoryCollectiveDiary();
                    break;
                case ExpansionNodeType.WorkshopTwinBlackSwanOrderSecret:
                    HandleWorkshopTwinBlackSwanOrder();
                    break;
                case ExpansionNodeType.PatronageDinnerSecretMinutes:
                    HandlePatronageDinnerMinutes();
                    break;
                case ExpansionNodeType.IsadoraSpyLatticeSecret:
                    HandleIsadoraSpyLattice();
                    break;
                case ExpansionNodeType.MezzanineImperialBalconyOverview:
                    HandleMezzanineOverview();
                    break;
                case ExpansionNodeType.TheoryGlassSlideProjectorSecret:
                    HandleTheoryProjectorSecret();
                    break;
                case ExpansionNodeType.MaestroKrolAlteredScoreSecret:
                    HandleMaestroKrolScoreSecret();
                    break;
                case ExpansionNodeType.ClockTowerJammedGearSecret:
                    HandleClockTowerSecret();
                    break;
                case ExpansionNodeType.BoilerIncinerationFurnaceSecret:
                    HandleBoilerFurnaceSecret();
                    break;
                case ExpansionNodeType.SubterraneanLaundryDumbwaiterSecret:
                    HandleLaundryDumbwaiterSecret();
                    break;
                case ExpansionNodeType.UnderstageTrapdoorMechanismSecret:
                    HandleUnderstageTrapdoorSecret();
                    break;
                case ExpansionNodeType.AncientCryptFoundersPassageSecret:
                    HandleAncientCryptSecret();
                    break;
                case ExpansionNodeType.CarriageHouseConfiscatedTrunkSecret:
                    HandleCarriageHouseSecret();
                    break;
                case ExpansionNodeType.GatehouseCutSwitchboardSecret:
                    HandleGatehouseSwitchboardSecret();
                    break;
                case ExpansionNodeType.FrozenSwanPondGazeboSecret:
                    HandleSwanPondSecret();
                    break;
            }
        }

        private void HandleBasementAcousticPipes()
        {
            hasInteracted = true;
            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(0f, 0f, 35f);
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Acústica Subterránea — Conductos del Sótano de Mantenimiento",
                    "Al apoyar el oído junto a las tuberías maestras de cobre y hierro que ascienden por los muros de carga:\n\n" +
                    "• A través del metal y del hormigón se filtran con inquietante claridad los sonidos de la academia: el roce rítmico de zapatillas de ballet practicando un *pas de deux* sobre las tablas superiores y una melodía de piano distante que viaja por las cañerías de vapor.\n" +
                    "• Sin embargo, el conducto que más vibra no es el de las aulas de ensayo, sino la **tubería norte que alimenta directamente el subsuelo clausurado del Teatro Principal**.");
            }
        }

        private void HandleBasementSecretLog()
        {
            hasInteracted = true;
            PlayInspectionChime(293.66f);

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL SÓTANO — Libro de Guardia y Contadores de Mantenimiento (Oct.–Nov. 1924)",
                    "REGISTRO TÉCNICO DE CALDERAS Y SUMINISTRO ELÉCTRICO — SUBSUELO DE LA ACADEMIA\n\n" +
                    "• **14 de Octubre de 1924 (23:55 h)**: «Orden de Dirección: Apagar iluminación general del Patio de Butacas tras el incidente del tercer acto.»\n" +
                    "• **16 de Octubre de 1924**: «Los inspectores municipales precintaron las puertas principales del teatro. Oficialmente el recinto queda clausurado y sin actividad.»\n\n" +
                    "• **ANOMALÍA REGISTRADA (18 de Octubre al 24 de Noviembre de 1924)**:\n" +
                    "  «El contador secundario **Sector 8 — Subsuelo y Trasera del Escenario** sigue marcando consumo constante de electricidad y calefacción de vapor todas las noches entre las 00:00 h y las 04:00 h.\n" +
                    "  Cuando intenté cerrar la llave de paso creyendo que era una fuga, la propia **Isadora Vale** bajó al sótano y me ordenó bajo amenaza de despido mantener abierta la válvula hacia la cámara oculta detrás del escenario. Esa sección del teatro siguió utilizándose en secreto mucho después de su supuesta clausura.»");
            }
        }

        private void HandleRooftopSecretWindow()
        {
            hasInteracted = true;
            PlayInspectionChime(392.00f);

            if (targetLight != null)
            {
                targetLight.intensity = 4.6f;
            }
            if (hiddenObjectTarget != null)
            {
                hiddenObjectTarget.SetActive(true);
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA AZOTEA — La Ventana que No Existe en los Planos",
                    "MIRADOR NORTE DE LA TERRAZA — PERSPECTIVA ELEVADA DEL TEATRO\n\n" +
                    "Al observar desde este ángulo específico de la balaustrada hacia el volumen posterior del Teatro Principal y compararlo con la copia del plano arquitectónico de Administración:\n\n" +
                    "• En los planos oficiales firmados por la dirección, el muro noreste detrás del escenario aparece dibujado como un **bloque macizo de contención sin ninguna estancia interior**.\n" +
                    "• Sin embargo, desde la azotea se distingue claramente **una ventana estrecha y enrejada a media altura, tibiamente iluminada desde el interior**, situada exactamente detrás de la pared del escenario y sobre el archivo subterráneo.\n" +
                    "• Alguien ocultó deliberadamente en los planos de Administración toda una **Zona Secreta (Sección 8)** adosada al teatro.");
            }
        }

        private void HandleInfirmaryScreenAnomaly()
        {
            hasInteracted = true;
            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(0f, -42f, 0f);
            }
            if (hiddenObjectTarget != null)
            {
                hiddenObjectTarget.SetActive(false);
            }
            if (localAudio != null && localAudio.isPlaying)
            {
                localAudio.Stop();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Biombo de Observación Nº 3 — Camilla Vacía",
                    "Hace apenas un instante, la lámpara clínica proyectaba sobre la tela marfil del biombo la silueta nítida de una bailarina sentada al borde de la camilla vendándose los tobillos.\n\n" +
                    "Al apartar el biombo no hay nadie. Sobre la sábana aún hundida quedan **gasas con restos de colofonia**, un frasco de sales de amoníaco destapado y una **cinta de satén negro cortada apresuradamente**.");
            }
        }

        private void HandleInfirmaryTornRegister()
        {
            hasInteracted = true;
            PlayInspectionChime(329.63f);

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA ENFERMERÍA — Libro de Ingresos Clínicos (Folio 88 — Página Siguiente Arrancada)",
                    "REGISTRO RESERVADO DE LA ENFERMERÍA — DOCTORA M. KOVACS\n\n" +
                    "• **Folio 88 — Noche del 14 de Octubre de 1924 (23:35 h)**:\n" +
                    "  «Paciente: **Elena Vesper (Solista Principal)**. Ingresa acompañada en secreto por la alumna **Mara Bell** por la puerta lateral tras finalizar la representación de *El Lago de las Sombras*.\n" +
                    "  Estado clínico: Vivo agotamiento físico, abrasiones por cuerda en ambas palmas y crisis de angustia aguda. La paciente repite que **el accidente del contrapeso en el escenario fue provocado** y que la Directora Vale pretende encerrarla esta misma noche para que no hable con las autoridades.\n" +
                    "  He decidido ocultarla temporalmente y dejar constancia firmada de su traslado a la estancia segura en la página siguiente por si algo me sucede...»\n\n" +
                    "• **[FOLIO 89 — ARRANCADO]**:\n" +
                    "  **La página siguiente ha sido cortada limpiamente con un bisturí quirúrgico.** En el borde dentado que quedó preso en la encuadernación solo sobreviven las primeras letras de cuatro renglones escritos con la misma tinta:\n" +
                    "  *«15 de oct... / Viva en... / Cám. tras el esc... / Mara lleva la llav...»*");
            }
        }

        private void HandleMemorialRemovedPlaque()
        {
            hasInteracted = true;
            PlayInspectionChime(261.63f);

            if (hiddenObjectTarget != null)
            {
                hiddenObjectTarget.SetActive(true);
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA SALA CONMEMORATIVA — La Placa Retirada del Muro de Honor",
                    "MURO DE LAS PROMOCIONES DESTACADAS (1919–1924)\n\n" +
                    "• En el centro del panel de caoba, flanqueado por flores secas y placas de latón bruñido, **una placa fue arrancada de la pared**. El barniz más oscuro que quedó protegido de la luz conserva su contorno rectangular exacto, los cuatro agujeros de los tornillos y el cerco verdoso de dos iniciales: **«E. V.»**.\n\n" +
                    "• Al inspeccionar la ranura inferior detrás del banco de madera situado bajo el muro, asoma la **placa de latón original retirada y oculta**:\n" +
                    "  *«EN HONOR A ELENA VESPER — Por su inolvidable interpretación privada en esta Academia la noche del 17 de Octubre de 1924.»*\n\n" +
                    "• Nota manuscrita adherida al reverso:\n" +
                    "  «Retirar inmediatamente por orden de Administración. Nadie del público debe saber que hubo una función a puerta cerrada tres días después de su desaparición oficial.»");
            }
        }

        private void HandleDormitoryMusicBox()
        {
            hasInteracted = true;
            if (localAudio != null && localAudio.isPlaying)
            {
                localAudio.Stop();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Caja de Música de Mara Bell — Cama Nº 4 del Dormitorio Común",
                    "Una pequeña caja de música de madera lacada con una figurilla de porcelana vestida de blanco.\n\n" +
                    "La cuerda estaba recién dada, aunque el dormitorio lleva años vacío. Debajo de la base hay una inscripción grabada con alfiler:\n" +
                    "«Regalo de Elena para Mara — Otoño de 1922: *Aunque nos obliguen a competir por el mismo papel, nunca bailaremos la una contra la otra*.»");
            }
        }

        private void HandleDormitoryCollectiveDiary()
        {
            hasInteracted = true;
            PlayInspectionChime(349.23f);

            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(-38f, 0f, 0f);
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL DORMITORIO DE INTERNAS — Diario Colectivo Escondido en el Baúl del Rincón",
                    "CUADERNO SECRETO DE LAS INTERNAS DEL CUERPO DE BAILE (OCTUBRE DE 1924)\n\n" +
                    "• **Sofía Lindqvist (15 de Octubre, 02:15 h)**: «Nos encerraron con llave en el dormitorio después de la función y nos dijeron que Elena había huido de la academia. Es mentira: su abrigo de invierno y su maleta siguen en su cuarto.»\n" +
                    "• **Clara Valmont (16 de Octubre, 03:10 h)**: «He visto a **Mara Bell** levantarse de su cama sin hacer ruido. Llevaba un paquete con pan del comedor, una botella de agua y vendas limpias de la enfermería. Salió hacia el taller de sastrería y la Sala de Poleas.»\n" +
                    "• **Valentina Morozova (17 de Octubre, 23:50 h)**: «Esta noche el piano del teatro volvió a sonar. Mara regresó llorando al amanecer y nos pidió que, si mañana nos obligan a abandonar el internado, escondamos este cuaderno donde la Directora Vale jamás lo encuentre.»");
            }
        }

        private void HandleWorkshopTwinBlackSwanOrder()
        {
            hasInteracted = true;
            PlayInspectionChime(311.13f);

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL TALLER Y TINTORERÍA — El Encargo de los Dos Cisnes Negros (2 de Octubre de 1924)",
                    "LIBRO RESERVADO DE PATRONAJE, TINTORERÍA Y HORMAS ESCÉNICAS\n\n" +
                    "• **Orden Confidencial Nº 104 (Fecha: 2 de Octubre de 1924 — DOCE DÍAS ANTES DEL ESTRENO)**:\n" +
                    "  «Por instrucción personal de la Directora **Isadora Vale**, se ordena confeccionar y teñir en baño de azabache **DOS trajes completos e idénticos del Cisne Negro**:\n" +
                    "  — Traje A: Medidas de **Elena Vesper** (Solista Titular).\n" +
                    "  — Traje B: Medidas de **Mara Bell**, añadiendo **alzas internas de corcho de 2,5 cm en las zapatillas de punta** y el mismo tocado de plumas negras para que desde el Patio de Butacas ambas bailarinas tengan exactamente la misma silueta y estatura.»\n\n" +
                    "• **Conclusión del Maestro Sastre**:\n" +
                    "  «La sustitución de Elena en mitad de la representación del 14 de octubre no fue una emergencia imprevista: la dirección planeó el intercambio de las dos bailarinas en escena casi dos semanas antes.»");
            }
        }

        private void HandlePatronageDinnerMinutes()
        {
            hasInteracted = true;
            PlayInspectionChime(277.18f);

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL SALÓN DEL PATRONATO — Acta de Medianoche y Bandejas al Subsuelo",
                    "MESA PRESIDENCIAL DEL PATRONATO — 14 AL 18 DE OCTUBRE DE 1924\n\n" +
                    "• **Acta Reservada del Patronato (14 de Octubre, 23:55 h)**:\n" +
                    "  «Reunidos de urgencia en este Salón de Té tras el cierre abrupto del tercer acto, la Directora Isadora Vale informa a los mecenas de que la primera bailarina Elena Vesper se negó a aceptar las condiciones del contrato de gira y amenazó con revelar las irregularidades contables del Patronato.\n" +
                    "  Se acuerda por unanimidad declarar oficialmente su \"desaparición voluntaria\" esa misma noche y clausurar temporalmente el teatro.»\n\n" +
                    "• **Nota del Montacargas de Cocina (16 de Octubre)**:\n" +
                    "  «Por tercera noche consecutiva han subido por el montacargas de servicio una bandeja con dos raciones intactas hacia el ala trasera del teatro. Quienquiera que siga encerrada detrás del escenario no está sola: alguien de dentro la está ayudando.»");
            }
        }

        private void HandleIsadoraSpyLattice()
        {
            hasInteracted = true;
            PlayInspectionChime(246.94f);

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA ESTANCIA DE ISADORA VALE — La Celosía de Vigilancia Oculta",
                    "GALERÍA DE OBSERVACIÓN TRAS LA BOISERIE — BITÁCORA PERSONAL DE ISADORA VALE\n\n" +
                    "A través de la rejilla de latón disimulada en el muro se observa directamente el interior de la Enfermería y el paso hacia Administración.\n\n" +
                    "• **Anotación del 13 de Octubre de 1924**: «Desde esta celosía escuché a Elena y a Mara hablar en voz baja. No son rivales como yo creía: han descubierto que mandé alterar el contrapeso del escenario y planean denunciarme tras la función de mañana.»\n" +
                    "• **Anotación del 14 de Octubre de 1924 (23:42 h)**: «Las veo a través de la rejilla de la enfermería. Mara acaba de traer a Elena herida en las manos por el cable de la polea. La doctora Kovacs ha escrito el ingreso en el folio 88 y habla de esconderla en la cámara detrás del escenario.\n" +
                    "  En cuanto el edificio quede en silencio, entraré en la enfermería, arrancaré el folio 89 con el bisturí y sellaré yo misma el acceso a la Zona Secreta.»");
            }
        }

        private void HandleMezzanineOverview()
        {
            hasInteracted = true;
            PlayInspectionChime(329.63f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "GALERÍA SUPERIOR DEL VESTÍBULO (ZONA 49) — Protocolo de Vigilancia del Entreacto",
                    "BALCÓN IMPERIAL DE LA PLANTA ALTA — LIBRO DE CEREMONIAL (14 DE OCTUBRE DE 1924)\n\n" +
                    "• Desde esta balaustrada elevada se dominan los accesos de la planta principal y el corredor que enlaza la Terraza (Zona 42) con las Aulas de Teoría (Zona 50) y la Residencia de Profesores (Zona 51).\n\n" +
                    "• **Anotación del Jefe de Sala (20:10 h)**:\n" +
                    "  «Cinco minutos antes de que se detuvieran todos los relojes del edificio, la Directora Isadora Vale mandó desalojar la Galería Superior para que ningún invitado viera al Maestro Viktor Krol subir apresuradamente hacia la Torre del Reloj con una llave de hierro.»");
            }
        }

        private void HandleTheoryProjectorSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(369.99f);
            if (targetLight != null) targetLight.intensity = 4.5f;
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LAS AULAS DE TEORÍA Y ANATOMÍA (ZONA 50) — Placas de Cristal Comparativas",
                    "ARCHIVO DE LINTERNA MÁGICA — ESTUDIO BIOMECÁNICO DE SALTO (OCTUBRE DE 1924)\n\n" +
                    "Al accionar el obturador de latón del proyector sobre el lienzo del anfiteatro aparecen dos placas fotográficas de vidrio superpuestas:\n\n" +
                    "• **Placa A (Elena Vesper — 1,68 m)**: Eje de caída del *grand jeté* en el centro exacto del escotillón del escenario.\n" +
                    "• **Placa B (Mara Bell — 1,655 m con alzas de 2,5 cm)**: Anotación manuscrita de Isadora Vale al margen:\n" +
                    "  «Si Mara entra desde el bastidor izquierdo en el compás 48 mientras la trampilla del foso desciende con Elena, el ángulo visual desde los palcos del Patronato hará imposible distinguir que la bailarina que termina el acto ya no es la misma que inició el salto.»");
            }
        }

        private void HandleMaestroKrolScoreSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(293.66f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA RESIDENCIA DEL MAESTRO VIKTOR KROL (ZONA 51) — El Compás del Escotillón",
                    "APOSENTOS DEL DIRECTOR DE ORQUESTA — BORRADOR CONFIDENCIAL DEL ACTO III\n\n" +
                    "Sobre el atril junto al armonio reposa la partitura de dirección original con correcciones en tinta roja:\n\n" +
                    "• **Compás 47–48 (Variación del Cisne Negro)**:\n" +
                    "  «Añadir golpe fortísimo de timbal y platillos exactamente en el segundo tiempo del compás 48. **Propósito acústico ordenado por Dirección**: amortiguar por completo el chasquido metálico del cerrojo del Foso Bajo el Escenario (Zona 55) al abrirse bajo los pies de Elena Vesper.»\n\n" +
                    "• **Carta sin enviar de Viktor Krol**:\n" +
                    "  «No puedo cargar con esta culpa. Me hicieron creer que el descenso por la trampilla era solo un efecto escénico para retirar a Elena antes del aplauso final, pero el golpe seco que oí bajo las tablas desde el foso de orquesta no fue un ensayo.»");
            }
        }

        private void HandleClockTowerSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(440.0f);
            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(0f, 0f, -25f);
            }
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA TORRE DEL RELOJ (ZONA 52) — El Engranaje Bloqueado a las 20:15",
                    "MAQUINARIA CENTRAL DEL RELOJ DE LA ACADEMIA — CONEXIÓN CON LA PASARELA DEL TEATRO\n\n" +
                    "Entre los grandes tambores de bronce que gobiernan los relojes esclavos de Recepción y Vestuarios hay **una cuña de hierro forjado clavada deliberadamente entre los dientes del escape principal**:\n\n" +
                    "• El reloj maestro no se detuvo a las **20:15 h** por avería. Fue **atascado a propósito a las 22:40 h tras haber retrasado manualmente las agujas dos horas y veinticinco minutos**.\n" +
                    "• Etiqueta de mantenimiento atada al péndulo:\n" +
                    "  «Orden de Isadora Vale: Fijar todos los relojes de la academia a las 20:15 h para establecer como coartada oficial que la función se suspendió al inicio del primer acto y que Elena jamás llegó a bailar el tercer acto.»");
            }
        }

        private void HandleBoilerFurnaceSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(220.0f);
            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(0f, 35f, 0f);
            }
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA SALA DE CALDERAS (ZONA 53) — Incineración de Medianoche",
                    "HOGAR DE COMBUSTIÓN DE LA CALDERA N.º 3 — NIVEL SUBTERRÁNEO (-3.20 m)\n\n" +
                    "Al entreabrir la pesada puerta de hierro fundido del cenicero inferior, entre la escoria de carbón apagada aparecen objetos que no llegaron a arder por completo:\n\n" +
                    "• **Un fragmento chamuscado del tutú original de Elena Vesper** con el gancho de latón arrancado de cuajo.\n" +
                    "• **El frasco vacío de cloroformo e hidrato de cloral** retirado de la Enfermería Antigua (Zona 43).\n" +
                    "• **Parte del Fogonero Nocturno**: «A la 01:10 h del 15 de octubre me ordenaron avivar el tiro de la Caldera N.º 3 y quemar un saco de lona bajado desde la Lavandería Subterránea.»");
            }
        }

        private void HandleLaundryDumbwaiterSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(261.63f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA LAVANDERÍA SUBTERRÁNEA (ZONA 54) — Terminal Inferior del Montacargas",
                    "POZO VERTICAL DEL MONTACARGAS (CONEXIÓN CON EL COMEDOR ZONA 47) Y TINAS DE VAPOR\n\n" +
                    "Justo en la vertical del Oficio del Comedor (`X = -60.8m, Z = 29.3m`) desemboca la caja metálica del montacargas subterráneo:\n\n" +
                    "• Dentro del cajón inferior hay **sábanas con el sello de la Enfermería manchadas de sangre oxidada y resina de ballet**, junto con una bandeja de plata devuelta desde los túneles del norte.\n" +
                    "• En el borde de madera del montacargas alguien grabó con la hebilla de una zapatilla desde el subsuelo:\n" +
                    "  *«Mara: si recibes esta bandeja arriba en el comedor, baja por la Cripta Antigua hasta el Foso Bajo el Escenario. Sigo con vida detrás del muro norte.» — E. V.*");
            }
        }

        private void HandleUnderstageTrapdoorSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(196.0f);
            if (primaryTarget != null)
            {
                primaryTarget.localRotation = initialRotA * Quaternion.Euler(-30f, 0f, 0f);
            }
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL FOSO BAJO EL ESCENARIO (ZONA 55 — UNDERSTAGE) — La Trampilla del Cisne",
                    "ESTRUCTURA INFERIOR BAJO LAS TABLAS DEL ESCENARIO PRINCIPAL (-3.20 m)\n\n" +
                    "Al alzar la vista entre el bosque de vigas de roble se ve directamente la cara inferior de la **Trampilla Central de Apariciones (Star Trap)** del Escenario Principal (Zona 32):\n\n" +
                    "• Los dos pasadores de seguridad que debían mantener firme el suelo durante el salto de Elena **no se rompieron por fatiga**: tienen marcas frescas de maza y grasa negra en las guías para que el pestillo cediera instantáneamente al tirar de la palanca inferior.\n" +
                    "• En el colchón amortiguador del foso quedó enganchado **un medallón de plata con el retrato de la madre de Elena** y el rastro de pasos arrastrados hacia la Cripta y la Escalera Oculta (Zona 36).");
            }
        }

        private void HandleAncientCryptSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(246.94f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LOS CIMIENTOS Y CRIPTA DEL SIGLO XVIII (ZONA 56) — El Conducto del Convento",
                    "BÓVEDAS FUNDACIONALES DE 1768 BAJO EL EJE CENTRAL DE LA ACADEMIA\n\n" +
                    "La Academia Vesper se edificó en 1889 aprovechando los cimientos subterráneos de un antiguo convento de clausura:\n\n" +
                    "• Sobre el altar de piedra secular reposa un plano en pergamino que muestra cómo **esta galería subterránea une en línea recta la Sala de Calderas (Sur) con el Foso Bajo el Escenario (Norte)**, pasando exactamente bajo el Vestíbulo y el Pasillo Principal.\n" +
                    "• Las huellas recientes de cal y cera en las losas demuestran que por este túnel trasladaron a Elena la noche del 14 de octubre sin cruzar jamás los pasillos superiores.");
            }
        }

        private void HandleCarriageHouseSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(311.13f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DE LA COCHERA DE CARRUAJES (ZONA 57) — El Baúl que Nunca Salió hacia Viena",
                    "PABELLÓN EXTERIOR DE CARRUAJES Y CARGA NORTE — EQUIPAJE RETENIDO\n\n" +
                    "Detrás del antiguo carruaje negro de gala de la academia se oculta un baúl de viaje precintado con lacre de Dirección:\n\n" +
                    "• En su interior están doblados el abrigo de viaje de **Elena Vesper**, su pasaporte y **dos billetes de ferrocarril nocturno hacia Viena con fecha del 15 de octubre de 1924 a las 06:00 h** (uno a nombre de Elena Vesper y otro a nombre de Mara Bell).\n" +
                    "• Esto desmiente por completo la versión oficial de Isadora Vale: Elena no huyó de madrugada; su equipaje y sus documentos fueron confiscados antes de que pudiera abandonar el recinto.");
            }
        }

        private void HandleGatehouseSwitchboardSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(349.23f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL PABELLÓN DEL GUARDA (ZONA 58) — Las Líneas Telefónicas Cortadas",
                    "CENTRALITA EXTERIOR DE CLAVIJAS Y LIBRO DE RONDA DEL GUARDA NOCTURNO\n\n" +
                    "Los cables trenzados de cobre que conectan la academia con la línea general de la ciudad fueron **cortados limpiamente con cizalla desde este pabellón exterior**:\n\n" +
                    "• **Registro de Centralita (14 de Octubre de 1924)**:\n" +
                    "  — **22:18 h**: «Intento de llamada saliente desde el teléfono de Recepción Antigua (Zona 6) solicitando a la comisaría del distrito y a una ambulancia. **Llamada desconectada por orden directa de Madame Vale**.»\n" +
                    "  — **22:22 h**: «Madame Vale se presenta en el Pabellón del Guarda, ordena cortar el puente de línea exterior y cerrar con candado el portón hasta nueva orden.»");
            }
        }

        private void HandleSwanPondSecret()
        {
            hasInteracted = true;
            PlayInspectionChime(293.66f);
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "SECRETO DEL ESTANQUE EXTERIOR Y QUIOSCO DEL PARQUE (ZONA 59) — El Espejo del Lago",
                    "EMBARCADERO DEL ESTANQUE DE LOS CISNES Y TEMPLETE DE MÚSICA (EXTERIOR NORESTE)\n\n" +
                    "Junto a los peldaños de piedra sumergidos en el agua helada flota una corona de flores secas y un estuche de cuero lastrado con piedras que la corriente devolvió a la orilla:\n\n" +
                    "• Dentro del estuche impermeable se conserva **la grabación en cilindro de cera original y sin editar del ensayo general** y una nota envuelta en hule firmada por **Mara Bell**:\n" +
                    "  *«Si alguien llega hasta el parque exterior, que sepa la verdad: el lago del escenario fue una trampa preparada por Isadora Vale y el Patronato. He dejado abiertas las rutas del subsuelo y de la planta alta para que quien entre después de mí pueda sacarla de la cámara detrás del escenario.»*");
            }
        }

        private void PlayInspectionChime(float freq)
        {
            if (localAudio == null) return;
            int sampleRate = 22050;
            int samples = sampleRate;
            AudioClip clip = AudioClip.Create("ExpansionSecretChime", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-3.0f * t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f
                         + Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * 0.35f) * env * 0.25f;
            }
            clip.SetData(data, 0);
            localAudio.clip = clip;
            localAudio.Play();
        }

        private AudioClip CreateBasementWallResonanceClip()
        {
            int sampleRate = 22050;
            float duration = 8.0f;
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            AudioClip clip = AudioClip.Create("BasementWallResonance", totalSamples, 1, sampleRate, false);
            float[] data = new float[totalSamples];

            float[] distantNotes = { 220.0f, 261.63f, 329.63f, 293.66f, 246.94f, 220.0f };
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float pipeHum = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.08f
                              + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.04f;

                float stepPhase = (t % 0.65f) / 0.65f;
                float muffledFootstep = Mathf.Exp(-24f * stepPhase) * Mathf.Sin(2f * Mathf.PI * 82f * stepPhase) * 0.14f;

                int noteIdx = Mathf.FloorToInt(t / 1.33f) % distantNotes.Length;
                float localNoteT = t % 1.33f;
                float noteEnv = Mathf.Exp(-2.2f * localNoteT);
                float muffledPiano = Mathf.Sin(2f * Mathf.PI * distantNotes[noteIdx] * t) * noteEnv * 0.06f;

                data[i] = Mathf.Clamp(pipeHum + muffledFootstep + muffledPiano, -0.35f, 0.35f);
            }

            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateFaintBreathingAndRustleClip()
        {
            int sampleRate = 22050;
            float duration = 5.0f;
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            AudioClip clip = AudioClip.Create("InfirmaryScreenBreath", totalSamples, 1, sampleRate, false);
            float[] data = new float[totalSamples];
            System.Random rng = new System.Random(19241014);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float breathCycle = Mathf.Sin(2f * Mathf.PI * 0.42f * t);
                float env = Mathf.Max(0f, breathCycle);
                float white = ((float)rng.NextDouble() * 2f - 1f) * 0.05f;
                float tone = Mathf.Sin(2f * Mathf.PI * 310f * t) * 0.015f;
                data[i] = (white + tone) * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateDetunedMusicBoxClip()
        {
            int sampleRate = 22050;
            float duration = 6.0f;
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            AudioClip clip = AudioClip.Create("DormitoryMusicBox", totalSamples, 1, sampleRate, false);
            float[] data = new float[totalSamples];

            float[] notes = { 523.25f, 659.25f, 587.33f, 493.88f, 523.25f, 440.0f };
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.FloorToInt(t) % notes.Length;
                float localT = t % 1.0f;
                float env = Mathf.Exp(-4.5f * localT);
                float detune = Mathf.Sin(2f * Mathf.PI * 1.2f * t) * 3.5f;
                data[i] = Mathf.Sin(2f * Mathf.PI * (notes[idx] + detune) * localT) * env * 0.14f;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
