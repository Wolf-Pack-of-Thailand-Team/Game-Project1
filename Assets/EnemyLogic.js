#pragma strict

var Health : int = 100;

function Update ()
{
	if (Health <= 0)
	{
		Dead();
	}
}
function DamageReciver (Damage : int)
{
	Health -= Damage;
}
function Dead()
{
	Destroy (gameObject);
}