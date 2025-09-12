```bash
docker run -it --rm -p 8000:8000 ghcr.io/magohl/carapi:latest
```

# prereqs cli tools

- kubectl
- kind
- helm

```bash
# create cluster
kind create cluster

# install crossplane in cluster
helm repo add crossplane-stable https://charts.crossplane.io/stable
helm repo update
helm install crossplane \
--namespace crossplane-system \
--create-namespace crossplane-stable/crossplane

# install crossplane provider for http
kubectl apply -f .manifests/crossplane/providers/provider.yaml
kubectl apply -f .manifests/crossplane/providers/functions.yaml
kubectl apply -f .manifests/crossplane/providers/providerconfig.yaml

# install our custom crossplane resource 'Car'
kubectl apply -f .manifests/crossplane/car/.

# deploy the api (if not hosted elsewhere)
kubectl create namespace carapi
kubectl apply -f .manifests/carapi/.

# place an order for a new car. But check the api first to see it change!
kubectl apply -f .manifests/crossplane/car/test/some-car.yaml
```

```bash
kubectl run -it --rm=true --image=quay.io/curl/curl:latest curl -- /bin/sh
curl -Lk -X GET http://carapi-service.carapi.svc.cluster.local:8000/api/cars
curl -Lk -X DELETE http://carapi-service.carapi.svc.cluster.local:8000/api/cars/...
```

## runbook

- Deploy car-api
- Deploy crossplane XRD & Composition
- Deploy an order for a car
