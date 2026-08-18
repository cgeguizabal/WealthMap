# Política de Privacidad de WealthMap

**Versión 1.0** · **Vigente desde el 18 de agosto de 2026**

---

## 1. En resumen

WealthMap es una aplicación para llevar tus finanzas personales. Tú registras tus cuentas,
tarjetas, compras y metas, y la aplicación calcula cuánto puedes gastar sin quedarte corto.

- Todo lo que hay dentro lo escribiste tú. WealthMap no se conecta a tu banco, no lee tu correo
  y no importa transacciones de ningún lado.
- Los nombres, correos electrónicos y dígitos de tarjeta se cifran antes de guardarse en la base
  de datos.
- **El operador de WealthMap tiene las llaves de cifrado y, por lo tanto, puede leer tus datos.**
  Esto se explica con franqueza en la sección 5, porque afirmar lo contrario sería falso.
- No se vende nada y no se comparte nada con fines publicitarios.

## 2. Quién es responsable

WealthMap lo opera un desarrollador individual ("el operador", "nosotros").
Contacto: **cgeguizabal@gmail.com**.

Al ser un proyecto personal y no una empresa, no hay un delegado de protección de datos ni un
equipo de privacidad. Las solicitudes llegan a esa dirección y las atiende una sola persona.

## 3. Qué se recopila

### 3.1 Lo que tú proporcionas

Todo lo de esta lista lo escribes tú. No hay ninguna otra fuente.

| Categoría | Ejemplos | Para qué |
|---|---|---|
| Identidad de la cuenta | Nombre completo, correo electrónico, país, moneda de referencia | Crear tu cuenta, iniciar sesión y dar formato a montos y fechas |
| Credenciales | Contraseña | Se guarda únicamente como hash con sal — ver 5.3 |
| Cuentas bancarias | Nombre de la cuenta, banco, tipo, saldo, últimos cuatro dígitos, tarjeta de débito vinculada, notas | Los saldos con los que razona la aplicación |
| Tarjetas de crédito | Nombre de la tarjeta, banco, límite, saldo adeudado, tasa de interés, fechas de corte y pago, últimos cuatro dígitos, notas | Proyectar cuánto debes y cuándo |
| Gastos | Compras, montos, fechas, categorías, comercios, planes de cuotas, notas | Dar seguimiento al gasto y a los compromisos a plazos |
| Ingresos | Nombre del empleador, salario, días de pago, deducciones, ingresos adicionales | Proyectar el dinero que va a entrar |
| Obligaciones | Deudas, pagos realizados, metas de ahorro y de producto | Calcular qué está comprometido |
| Consentimiento | Qué versión de las políticas aceptaste, y cuándo | Tener constancia de tu aceptación |

### 3.2 Lo que el software registra por su cuenta

- **Tokens de sesión.** El token de renovación se guarda como hash para poder renovar y revocar
  una sesión. Los tokens expiran y rotan en cada uso.
- **Marcas de tiempo.** Cada registro guarda cuándo se creó y cuándo se actualizó.
- **Registros del servidor.** Los errores se registran con el método HTTP, la ruta y el
  identificador del registro involucrado. Deliberadamente **no** contienen nombres, correos,
  notas ni cuerpos de solicitud.

### 3.3 Lo que no se recopila

Ninguna analítica, ningún identificador publicitario, ningún píxel de rastreo, ningún script de
terceros y ninguna cookie más allá de la única cookie de sesión descrita en la sección 7. No hay
conexión bancaria ni lectura de correo: WealthMap no puede ver ninguna cuenta que tú no hayas
escrito.

## 4. Para qué se usa

Tus datos se usan para que WealthMap funcione para ti: mostrar tus saldos, proyectar tu liquidez,
calcular cuánto puedes gastar con seguridad, generar tu reporte mensual y avisarte dentro de la
aplicación sobre fechas de corte y de pago.

No se usan para entrenar modelos de aprendizaje automático, construir un perfil tuyo ni venderte
nada.

## 5. Cómo se protege — y hasta dónde llega esa protección

### 5.1 Cifrado en reposo

Estas columnas se cifran individualmente antes de escribirse, con AES-256-GCM y una llave de
256 bits:

- tu nombre completo, correo electrónico y país;
- nombres de cuentas, notas, dígitos de cuenta y dígitos de tarjeta de débito;
- nombres de tarjetas de crédito, notas y dígitos de tarjeta;
- nombres de deudas, notas de compras, nombres de metas de ahorro y de producto;
- títulos y mensajes de las notificaciones, y su contenido.

GCM es un modo autenticado, lo que significa que un valor alterado directamente en la base de
datos falla al descifrarse en lugar de devolver en silencio algo que parezca válido.

Tu correo electrónico se guarda además como un hash con llave, bajo una llave **distinta**, para
que el inicio de sesión pueda encontrar tu cuenta sin que la base de datos guarde una copia
consultable de la dirección.

### 5.2 Qué logra y qué no logra este cifrado

**Protege frente a una base de datos robada.** Una copia de la base de datos —un respaldo
filtrado, una cuenta de hosting comprometida, una instantánea mal configurada— no es legible sin
las llaves, y las llaves no están guardadas en ella.

**No pone tus datos fuera del alcance del operador.** La aplicación descifra tus datos en cada
pantalla que abres, así que las llaves viven en su configuración, y el operador controla esa
configuración. Esto es seudonimización, no cifrado de extremo a extremo ni de conocimiento cero.

Dicho sin rodeos: **el operador tiene la capacidad técnica de leer tus datos.** Quien te diga que
un diseño como este lo impide está describiendo otro sistema. Si no te sentirías cómodo con que
una persona pueda leer lo que escribes, no lo escribas.

### 5.3 Contraseñas

Tu contraseña se guarda únicamente como un hash con sal. No se cifra, porque cifrar implicaría que
se puede revertir. El operador no puede leer tu contraseña ni decirte cuál es: una contraseña
olvidada solo puede reemplazarse.

### 5.4 En tránsito

El tráfico viaja por HTTPS. Las cookies de sesión son HttpOnly y Secure, de modo que el JavaScript
del navegador no puede leerlas.

## 6. Dónde se guarda y quién más lo toca

La base de datos está alojada en **Neon** (PostgreSQL sin servidor). Neon actúa como encargado del
tratamiento: guarda los datos por cuenta del operador y no los usa para nada más. Las columnas
cifradas llegan a Neon ya cifradas.

Los datos no se comparten, venden ni divulgan a nadie más, con dos excepciones que cualquier
operador debe declarar: cumplir un requerimiento legal válido, e investigar un abuso o un incidente
de seguridad.

Según la región de alojamiento, los datos pueden almacenarse fuera de tu país.

## 7. Cookies

Una sola cookie: el token de renovación que te mantiene con la sesión iniciada. Es HttpOnly, Secure,
SameSite y expira. No hay cookies de analítica ni de publicidad, así que no hay nada que rechazar.

## 8. Cuánto tiempo se conserva

Tus datos se conservan mientras exista tu cuenta. Si la eliminas, los registros asociados se
eliminan con ella: cuentas, tarjetas, compras, metas y lo demás se borran en cascada en lugar de
quedar sueltos.

Los tokens de renovación expiran por su cuenta. Los registros del servidor se conservan solo el
tiempo que los retenga la plataforma de alojamiento.

## 9. Tus opciones

- **Verlos.** Cada pantalla te muestra tus propios datos; el reporte mensual los exporta en PDF.
- **Corregirlos.** Todo registro en WealthMap se puede editar.
- **Eliminarlos.** Los registros individuales se pueden borrar desde la aplicación. Para eliminar
  tu cuenta completa, escribe a la dirección de la sección 2.
- **Llevártelos.** Pídelo y se te enviarán tus datos en un formato legible por máquina.
- **Retirar el consentimiento.** Deja de usar WealthMap y solicita la eliminación.

Según dónde vivas, puedes tener derechos adicionales reconocidos por ley.

## 10. Menores de edad

WealthMap no está dirigido a menores de 16 años y no se usa conscientemente por ellos.

## 11. Cambios

Los cambios importantes elevan el número de versión que aparece arriba. La versión que aceptaste
queda registrada en tu cuenta, así que siempre es posible saber qué texto aceptaste.

## 12. Contacto

**cgeguizabal@gmail.com**
