---
layout: default
title: eDAIS Operational Considerations
---

# Operational Considerations

This document contains some handy hints around deployment and the behaviour of particular systems you may need to interoperate with. Please read the [specification](spec.html) for background & terminology first.

### Fault Handling

When an Application is Lodged for an eDAIS-enabled Certifier, the EHC will attempt to contact the endpoint configured for that Certifier. If this fails, a fault is raised for the support team at GLS. They will attempt a manual resend, and if this fails, attempt to reach the designated Council contact. Likewise for issues related to incoming messages, such as attachment downloading.

When a Certifier System fails to communicate successfully with the EHC, an alert must be raised for the attention of a human. Failure to send or receive communications could result in applications being lost, which will cripple trust in the ecosystem, and render all the good work useless.

### Private Certifier Systems

Any XML elements sent to the Private Certifier system upon Lodgement of an Application must be preserved and forwarded onto the Council System when an application is determined. As an example, the `<ABS_Statistics>` element must be sent to the EHC when an application is determined, whether or not that information is used by the Private Certifier system.

### Quirks

Due to the internal technical requirements of some of the systems in the eDAIS ecosystem, the EHC will send a `ReceiptAcknowledgmentSignal` prior to downloading the attachments contained in a `DeclareSaveDetermination`. Should the attachment download fail, a fault is raised, as described in the Fault Handling section.