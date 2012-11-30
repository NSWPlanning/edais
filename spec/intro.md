---
layout: default
title: eDAIS Introduction
---

# What Is eDAIS?

eDAIS is a SOAP-based system for transferring information about development applications between heterogeneous systems. This site covers the use of the specification in NSW and the modifications ('specialisations') employed.

There are three categories of actor in NSW:

* The Electronic Housing Code system, provided by [DP&I](http://www.planning.nsw.gov.au) and [GLS](http://www.licence.nsw.gov.au)
* Council systems, provided by commercial vendors
* Private Certifier systems, also provided by commercial vendors

Complying Development can be certified by either the Council of the area the development is being undertaken in, or by a Private Certifer who operates in that area. Private Certifiers do not need Council approval to Approve or Refuse an application, but they must notify Council two days prior to the commencement of building works.

## Use Cases

The process, from an applicant's point of view:

Number | Step                     | In Plain English 
-------|--------------------------|------------------
1      | Investigate              | Can I put in a pool?
2      | Prepare Application      | Upload plans, provide contact details.
3      | Submit for Consideration | Here's my application; check it over please. Certifier can Accept (take on the work) or Reject (refuse the work)
4      | Lodge Application        | Formally start processing this application. This is the point at which the CDC 10-day clock starts.
5      | Determine Application    | Approve or Refuse the application.


Note: A common point of confusion:

* Accept/Reject: Certifier decides if they wish to take on the business.
* Approve/Refuse: Certifier determines whether the application is allowed or not.

### Common Configurations

There are two primary configurations:
1. Council as Certifier
2. Private Certifier with Council


### EHC with Council as Certifier

Number | Step                     | eDAIS
-------|--------------------------|------
1      | Investigate              | n/a
2      | Prepare Application      | n/a
3      | Submit for Consideration | n/a 
4      | Lodge Application        | EHC sends application to Council system
5      | Determine Application    | Council system sends determination to the EHC

### EHC with Private Certifier and Council

Number | Step                     | eDAIS
-------|--------------------------|------
1      | Investigate              | n/a 
2      | Prepare Application      | n/a 
3      | Submit for Consideration | n/a 
4      | Lodge Application        | EHC sends application to Private Certifier system
5      | Determine Application    | Private Certifier system sends determination to the EHC and the Council system


## Transport Layer & Security

The transport layer is SOAP with BasicHttpBinding. Messages are sent and acknowledged synchronously.

SSL is enforced, and the EHC system can only connect to systems on the default SSL port (443).

Messages are additionally secured using [UsernameToken][1]. The username and password are configured on a per-endpoint basis, that is per-Council and per-Private Certifier. These usernames and passwords must be identically configured in any communicating systems.

{% highlight xml %}
<soapenv:Header>
    <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
      <u:Timestamp u:Id="89f3558b-91cd-426a-a7cc-e75e8a68eed2">
        <u:Created>2012-11-29T02:48:12.911Z</u:Created>
        <u:Expires>2012-11-29T02:53:12.911Z</u:Expires>
      </u:Timestamp>
      <wsse:UsernameToken>
        <wsse:Username>sample-username</wsse:Username>
        <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">sample-password</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </soapenv:Header>
  {% endhighlight %}


[1]: http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0.pdf

## Attachments







