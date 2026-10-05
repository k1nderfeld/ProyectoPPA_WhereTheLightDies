# Créditos de assets

Registro de todos los recursos que no son código del juego: autor, fuente, licencia y dónde están en el proyecto. **Todo asset nuevo se anota aquí** (regla del equipo, ver `docs/ESTRUCTURA_Y_CONVENCIONES.md`).

## Assets de terceros

| Asset | Autor | Fuente | Licencia | Ubicación |
|---|---|---|---|---|
| Low Poly Shooter Pack - Free Sample (modelos de armas, brazos, barril y tanque, materiales, texturas y sonidos de armas) | Infima Games | Unity Asset Store | Unity Asset Store EULA | `Assets/Terceros/Infima Games/` |

Notas sobre el pack:
- Se usa la versión gratuita (*Free Sample*).
- Los materiales se convirtieron a URP con el *Render Pipeline Converter* de Unity.
- Las carpetas de animaciones (`Animations` y `Animators`) están excluidas del repositorio por su peso (más de 170 MB). Se agregarán solo si se usan.
- El repositorio es privado; el pack solo se usa dentro de este proyecto, como permite la licencia del Asset Store.

## Assets propios del equipo

| Asset | Autor | Origen | Ubicación |
|---|---|---|---|
| Arte conceptual (vista en primera persona, protagonista y enemigos) | Equipo Goty Devs | Dibujos originales | `Assets/Arte/Concepto/` |
| Degradado del menú | Equipo Goty Devs | Generado con código | `Assets/Arte/Interfaz/` |
| Materiales de la escena del sótano | Equipo Goty Devs | Creados en Unity | `Assets/Arte/Materiales/` |
| Música de ambiente del menú (`ambiente_menu.wav`) | Equipo Goty Devs | Generada con código (síntesis); **provisoria** | `Assets/Audio/Musica/` |
| Sonidos de botones (`click.wav`, `hover.wav`) | Equipo Goty Devs | Generados con código (síntesis); **provisorios** | `Assets/Audio/Efectos/` |

Los sonidos del menú son originales y no tienen restricciones de uso. Están pensados para ser reemplazados por los definitivos del diseñador de sonido.

## Recursos incluidos en Unity

| Recurso | Licencia |
|---|---|
| Fuente por defecto de la interfaz (`LegacyRuntime`) | Incluida en el editor de Unity |
| Imágenes de sliders y casillas de la interfaz (`UI/Skin`) | Incluidas en el editor de Unity |
